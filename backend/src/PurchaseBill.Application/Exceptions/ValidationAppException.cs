namespace PurchaseBill.Application.Exceptions;

/// <summary>Thrown for request-level validation failures that don't go through FluentValidation's pipeline behavior.</summary>
public class ValidationAppException(IDictionary<string, string[]> errors) : Exception("Validation failed.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = new Dictionary<string, string[]>(errors);
}
