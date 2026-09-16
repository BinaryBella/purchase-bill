namespace PurchaseBill.Application.Dtos.Auth;

/// <summary>Login request submitted by the Angular login form.</summary>
public record LoginRequest(string Email, string Password);
