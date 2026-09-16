namespace PurchaseBill.Application.Dtos.PurchaseBills;

/// <summary>Payload submitted when saving the whole Purchase Bill (all grid rows at once).</summary>
public record CreatePurchaseBillRequest(IReadOnlyList<PurchaseBillItemRequest> Items);
