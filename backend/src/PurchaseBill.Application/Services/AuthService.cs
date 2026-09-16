using PurchaseBill.Application.Dtos.Auth;
using PurchaseBill.Application.Entities;
using PurchaseBill.Application.Exceptions;
using PurchaseBill.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PurchaseBill.Application.Services;

/// <summary>
/// Drives Task 1: authenticates against the external Enhanzer POS API, persists the
/// User_Locations it returns into Location_Details, and issues our own JWT for subsequent
/// calls to this API (the SPA never talks to Enhanzer directly).
///
/// The Enhanzer envelope has three confirmed shapes:
///   - valid credentials      -> Status_Code 200, Response_Body[0].User_Locations populated.
///   - known user, wrong pw   -> Status_Code 200, Response_Body[0].Doc_Msg = "Invalid Login Details".
///   - unknown company/user   -> Status_Code 401, Response_Body = null, top-level Message set.
/// </summary>
public class AuthService(
    IEnhanzerAuthClient enhanzerAuthClient,
    IApplicationDbContext db,
    IJwtTokenGenerator jwtTokenGenerator,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var envelope = await enhanzerAuthClient.GetLoginDataAsync(request.Email, request.Password, ct);

        // Observed on staging under back-to-back requests against the shared demo account:
        // Status_Code 0 / Message null / Response_Body null, rather than a real error - the
        // upstream API throttling itself rather than rejecting the credentials.
        if (envelope.StatusCode == 0 && envelope.ResponseBody is null && envelope.Message is null)
        {
            logger.LogWarning("Enhanzer login API returned an empty envelope for {Email}; likely throttled", request.Email);
            throw new AuthenticationFailedException("The authentication service is busy right now. Please wait a few seconds and try again.");
        }

        if (envelope.StatusCode != 200)
        {
            throw new AuthenticationFailedException(envelope.Message ?? "Login failed.");
        }

        var result = envelope.ResponseBody?.FirstOrDefault();
        if (result is null)
        {
            throw new AuthenticationFailedException("Login failed: no response from the authentication service.");
        }

        if (!string.IsNullOrWhiteSpace(result.DocMsg))
        {
            throw new AuthenticationFailedException(result.DocMsg);
        }

        if (result.UserLocations is null || result.UserLocations.Count == 0)
        {
            throw new AuthenticationFailedException("Login succeeded but no locations were returned for this user.");
        }

        await UpsertLocationsAsync(result.UserLocations, ct);

        var username = result.Email ?? request.Email;
        var (token, expiresAtUtc) = jwtTokenGenerator.GenerateToken(username, result.CompanyCode ?? string.Empty);

        logger.LogInformation("User {Username} logged in with {LocationCount} locations", username, result.UserLocations.Count);

        var locations = result.UserLocations
            .Select(l => new LocationDto(l.LocationCode, l.LocationName))
            .ToList();

        return new LoginResponse(token, expiresAtUtc, username, locations);
    }

    private async Task UpsertLocationsAsync(IEnumerable<Dtos.Enhanzer.EnhanzerUserLocation> locations, CancellationToken ct)
    {
        var incoming = locations
            .GroupBy(l => l.LocationCode)
            .Select(g => g.First())
            .ToList();

        var codes = incoming.Select(l => l.LocationCode).ToList();
        var existing = await db.LocationDetails
            .Where(l => codes.Contains(l.LocationCode))
            .ToDictionaryAsync(l => l.LocationCode, ct);

        var now = DateTime.UtcNow;

        foreach (var location in incoming)
        {
            if (existing.TryGetValue(location.LocationCode, out var record))
            {
                record.LocationName = location.LocationName;
                record.UpdatedAt = now;
            }
            else
            {
                db.LocationDetails.Add(new LocationDetail
                {
                    LocationCode = location.LocationCode,
                    LocationName = location.LocationName,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
