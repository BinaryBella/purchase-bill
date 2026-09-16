namespace PurchaseBill.Application.Dtos.PurchaseBills;

/// <summary>One row of the Items grid, as submitted from the Angular form.</summary>
public record PurchaseBillItemRequest(
    string ItemName,
    string BatchLocationCode,
    decimal StandardCost,
    decimal StandardPrice,
    decimal Quantity,
    decimal FreeQuantity,
    decimal DiscountPercent);
