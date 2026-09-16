namespace PurchaseBill.Application.Exceptions;

/// <summary>
/// Thrown when the Enhanzer login call reports invalid credentials or an unauthorized request.
/// Mapped to HTTP 401 by the API's exception-handling middleware.
/// </summary>
public class AuthenticationFailedException(string message) : Exception(message);
