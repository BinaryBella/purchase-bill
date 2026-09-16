namespace PurchaseBill.Application.Dtos.Auth;

/// <summary>Response returned to the Angular client after a successful login.</summary>
public record LoginResponse(string Token, DateTime ExpiresAtUtc, string Username, IReadOnlyList<LocationDto> Locations);

public record LocationDto(string LocationCode, string LocationName);
