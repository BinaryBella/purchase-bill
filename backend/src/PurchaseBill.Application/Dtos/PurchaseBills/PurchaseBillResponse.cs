namespace PurchaseBill.Application.Dtos.PurchaseBills;

public record PurchaseBillItemResponse(
    int Id,
    string ItemName,
    string BatchLocationCode,
    string BatchLocationName,
    decimal StandardCost,
    decimal StandardPrice,
    decimal Margin,
    decimal Quantity,
    decimal FreeQuantity,
    decimal DiscountPercent,
    decimal TotalCost,
    decimal TotalSelling);

/// <summary>Mirrors the "Item Summary" panel: total row count and summed quantity.</summary>
public record PurchaseBillResponse(
    int Id,
    DateTime CreatedAt,
    int TotalItems,
    decimal TotalQuantity,
    decimal TotalCost,
    decimal TotalSelling,
    IReadOnlyList<PurchaseBillItemResponse> Items);
