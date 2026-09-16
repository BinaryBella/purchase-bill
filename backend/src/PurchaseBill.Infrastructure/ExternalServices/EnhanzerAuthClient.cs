using System.Net.Http.Json;
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
/// </summary>
public class EnhanzerAuthClient(HttpClient httpClient, IOptions<EnhanzerOptions> options, ILogger<EnhanzerAuthClient> logger)
    : IEnhanzerAuthClient
{
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

        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync("api/External_Api/POS_Api/Invoke", payload, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogError(ex, "Enhanzer login API call failed for {Email}", email);
            throw new AuthenticationFailedException("Unable to reach the authentication service. Please try again.");
        }

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Enhanzer login API returned HTTP {StatusCode} for {Email}", response.StatusCode, email);
            throw new AuthenticationFailedException("Login failed.");
        }

        var envelope = await response.Content.ReadFromJsonAsync<EnhanzerApiEnvelope>(cancellationToken: ct);
        return envelope ?? throw new AuthenticationFailedException("Login failed: empty response from the authentication service.");
    }
}
