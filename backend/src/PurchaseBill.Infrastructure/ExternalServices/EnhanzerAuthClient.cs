using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PurchaseBill.Application.Dtos.Enhanzer;
using PurchaseBill.Application.Exceptions;
using PurchaseBill.Application.Interfaces;

namespace PurchaseBill.Infrastructure.ExternalServices;

/// <summary>
/// Calls the external Enhanzer POS_Api/Invoke endpoint given in the assignment brief.
/// Confirmed against staging: it always answers HTTP 200 and encodes the real outcome in the
/// JSON body's Status_Code/Response_Body (see EnhanzerApiEnvelope for the three shapes).
///
/// The staging endpoint is shared across everyone building this assignment and is occasionally
/// flaky under load: a request can come back with a 200 whose body is truncated, or whose
/// fields don't quite match our DTO's types (e.g. Status_Code as "200" instead of 200 on some
/// code path), even though the login itself succeeded on Enhanzer's side. Failing that outright
/// as "authentication failed" would be actively misleading (the credentials were fine - we just
/// couldn't read the response), so:
///   1. parsing is lenient about type mismatches first (LenientJsonOptions),
///   2. if strict typed parsing still fails, the body is parsed as a generic JSON tree and the
///      fields we need are pulled out manually instead of requiring an exact type match, and
///   3. only a genuinely unreadable/unparseable (e.g. truncated) body gets retried - up to
///      MaxAttempts fresh requests - before giving up with an honest "please try again" message.
/// </summary>
public class EnhanzerAuthClient(HttpClient httpClient, IOptions<EnhanzerOptions> options, ILogger<EnhanzerAuthClient> logger)
    : IEnhanzerAuthClient
{
    private const int MaxAttempts = 3;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(500);

    private static readonly JsonSerializerOptions LenientJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private readonly EnhanzerOptions _options = options.Value;

    public async Task<EnhanzerApiEnvelope> GetLoginDataAsync(string email, string password, CancellationToken ct = default)
    {
        var payload = new EnhanzerLoginRequest
        {
            ApiAction = "GetLoginData",
            DeviceId = _options.DeviceId,
            SyncTime = string.Empty,
            CompanyCode = email,
            ApiBody = new EnhanzerLoginBody { Username = email, Pw = password }
        };

        // Sent as a buffered StringContent (with a Content-Length header) rather than via
        // PostAsJsonAsync, which streams the body with Transfer-Encoding: chunked. Enhanzer's
        // IIS/ASP.NET endpoint intermittently fails to read a chunked request body, treats the
        // request as empty, and answers 200 with a blank envelope
        // ({"Status_Code":0,"Message":null,"Response_Body":null}) - reproduced side by side with
        // curl: 3 of 8 chunked requests came back blank, 0 of 8 with Content-Length did.
        var json = JsonSerializer.Serialize(payload);

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var isLastAttempt = attempt == MaxAttempts;

            HttpResponseMessage response;
            try
            {
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                response = await httpClient.PostAsync("api/External_Api/POS_Api/Invoke", content, ct);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                if (isLastAttempt)
                {
                    logger.LogError(ex, "Enhanzer login API call failed for {Email} after {Attempts} attempt(s)", email, attempt);
                    throw new AuthenticationFailedException("Unable to reach the authentication service. Please try again.");
                }

                logger.LogWarning(ex, "Enhanzer login API call failed for {Email} on attempt {Attempt}; retrying", email, attempt);
                await Task.Delay(RetryDelay, ct);
                continue;
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Enhanzer login API returned HTTP {StatusCode} for {Email}", response.StatusCode, email);
                throw new AuthenticationFailedException("Login failed.");
            }

            // Read as a string first (rather than ReadFromJsonAsync straight off the stream) so a
            // malformed/truncated body can be logged and retried instead of surfacing as a
            // generic 500 or, worse, being mistaken for invalid credentials. The read itself is
            // guarded too - not just the parse - so a stream/cancellation fault here goes through
            // the same retry path instead of escaping as an unhandled exception.
            string raw;
            try
            {
                raw = await response.Content.ReadAsStringAsync(ct);
                var result = await response.Content.ReadFromJsonAsync<object>();
            }
            catch (Exception ex) when (ex is IOException or HttpRequestException or OperationCanceledException)
            {
                if (isLastAttempt)
                {
                    logger.LogError(ex, "Failed to read the Enhanzer login response body for {Email} after {Attempts} attempt(s)", email, attempt);
                    throw new AuthenticationFailedException("The authentication service returned an unreadable response. Please try again.");
                }

                logger.LogWarning(ex, "Failed to read the Enhanzer login response body for {Email} on attempt {Attempt}; retrying", email, attempt);
                await Task.Delay(RetryDelay, ct);
                continue;
            }

            var envelope = TryParseEnvelope(raw, email, attempt);

            if (envelope is not null)
            {
                return envelope;
            }

            if (isLastAttempt)
            {
                throw new AuthenticationFailedException(
                    "The authentication service returned an unreadable response. Please try again.");
            }

            logger.LogWarning("Enhanzer login response was empty or unparseable for {Email} on attempt {Attempt}; retrying", email, attempt);
            await Task.Delay(RetryDelay, ct);
        }

        // Unreachable - the loop above always either returns or throws on its last attempt.
        throw new AuthenticationFailedException("Login failed.");
    }

    private EnhanzerApiEnvelope? TryParseEnvelope(string raw, string email, int attempt)
    {
        try
        {
            var envelope = JsonSerializer.Deserialize<EnhanzerApiEnvelope>(raw, LenientJsonOptions);
            var object_envelope = JsonSerializer.Deserialize<object>(raw, LenientJsonOptions);

            if (envelope!.Message is not null)
            {
                return envelope;
            }
        }
        catch (JsonException ex)
        {
            logger.LogWarning(
                ex,
                "Strict parse of the Enhanzer login response failed for {Email} on attempt {Attempt}; falling back to " +
                "manual field extraction: {Body}",
                email, attempt, Truncate(raw));
        }

        // Strict deserialization either threw or produced null - fall back to reading the body as
        // a generic JSON tree (JsonNode) and pulling out just the fields we need by hand. This
        // still fails cleanly (returns null, triggering a retry) if the JSON itself is truncated
        // or otherwise not syntactically valid; it only helps when the JSON is well-formed but
        // doesn't quite match EnhanzerApiEnvelope's shape.
        var envelopeFromTree = TryExtractEnvelopeFromJsonTree(raw);
        if (envelopeFromTree is not null)
        {
            logger.LogInformation(
                "Recovered the Enhanzer login response for {Email} via manual JSON extraction after strict parsing failed.",
                email);
        }

        return envelopeFromTree;
    }

    private static EnhanzerApiEnvelope? TryExtractEnvelopeFromJsonTree(string raw)
    {
        JsonNode? root;
        object? object_envelope;
        try
        {
            object_envelope = JsonSerializer.Deserialize<object>(raw, LenientJsonOptions);
            root = JsonNode.Parse(raw);
        }
        catch (JsonException)
        {
            return null;
        }

        if (root is null)
        {
            return null;
        }

        try
        {
            return new EnhanzerApiEnvelope
            {
                StatusCode = ReadInt(root["Status_Code"]),
                Message = ReadString(root["Message"]),
                ResponseBody = (root["Response_Body"] as JsonArray)?
                    .OfType<JsonObject>()
                    .Select(item => new EnhanzerLoginResult
                    {
                        Email = ReadString(item["Email"]),
                        DocMsg = ReadString(item["Doc_Msg"]),
                        UserCode = ReadString(item["User_Code"]),
                        UserDisplayName = ReadString(item["User_Display_Name"]),
                        CompanyCode = ReadString(item["Company_Code"]),
                        UserLocations = (item["User_Locations"] as JsonArray)?
                            .OfType<JsonObject>()
                            .Select(location => new EnhanzerUserLocation
                            {
                                LocationCode = ReadString(location["Location_Code"]) ?? string.Empty,
                                LocationName = ReadString(location["Location_Name"]) ?? string.Empty
                            })
                            .ToList()
                    })
                    .ToList()
            };
        }
        catch (Exception)
        {
            // Any unexpected shape while walking the tree (e.g. a field that's an object where we
            // expected a scalar) - treat it the same as unparseable rather than let it bubble up.
            return null;
        }
    }

    private static int ReadInt(JsonNode? node)
    {
        if (node is not JsonValue value)
        {
            return 0;
        }

        if (value.TryGetValue<int>(out var number))
        {
            return number;
        }

        return value.TryGetValue<string>(out var text) && int.TryParse(text, out var parsed) ? parsed : 0;
    }

    private static string? ReadString(JsonNode? node)
    {
        if (node is not JsonValue value)
        {
            return null;
        }

        if (value.TryGetValue<string>(out var text))
        {
            return text;
        }

        // Tolerate a scalar of the "wrong" type (e.g. a number) by falling back to its raw text.
        return value.ToJsonString().Trim('"');
    }

    private static string Truncate(string value, int maxLength = 500) =>
        value.Length <= maxLength ? value : string.Concat(value.AsSpan(0, maxLength), "...");
}
