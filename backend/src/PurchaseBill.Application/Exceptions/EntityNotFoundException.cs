namespace PurchaseBill.Application.Exceptions;

/// <summary>Mapped to HTTP 404 by the API's exception-handling middleware.</summary>
public class EntityNotFoundException(string message) : Exception(message);
