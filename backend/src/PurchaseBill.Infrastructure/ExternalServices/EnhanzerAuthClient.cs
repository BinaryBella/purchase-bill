using System.Net.Http.Json;
using System.Text.Json;
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
/// flaky under load: a request can come back with a 200 whose body is truncated or otherwise
/// not valid JSON, even though the login itself succeeded on Enhanzer's side. Failing that
/// outright as "authentication failed" would be actively misleading (the credentials were
/// fine - we just couldn't read the response), so a read/parse failure gets one retry with a
/// fresh request before giving up with an honest "please try again" message.
/// </summary>
public class EnhanzerAuthClient(HttpClient httpClient, IOptions<EnhanzerOptions> options, ILogger<EnhanzerAuthClient> logger)
    : IEnhanzerAuthClient
{
    private const int MaxAttempts = 2;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(500);

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

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var isLastAttempt = attempt == MaxAttempts;

            HttpResponseMessage response;
            try
            {
                response = await httpClient.PostAsJsonAsync("api/External_Api/POS_Api/Invoke", payload, ct);
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
            // generic 500 or, worse, being mistaken for invalid credentials.
            var raw = await response.Content.ReadAsStringAsync(ct);
            EnhanzerApiEnvelope? envelope = null;
            try
            {
                envelope = JsonSerializer.Deserialize<EnhanzerApiEnvelope>(raw);
            }
            catch (JsonException ex)
            {
                logger.LogWarning(
                    ex,
                    "Failed to parse the Enhanzer login response for {Email} on attempt {Attempt}: {Body}",
                    email, attempt, Truncate(raw));
            }

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

    private static string Truncate(string value, int maxLength = 500) =>
        value.Length <= maxLength ? value : string.Concat(value.AsSpan(0, maxLength), "...");
}
