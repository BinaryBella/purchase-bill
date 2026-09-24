namespace PurchaseBill.Application.Dtos.Dashboard;

/// <summary>Table widget row: one of the latest purchase orders.</summary>
public record LatestOrderDto(int Id, string PoNumber, decimal NetAmount, int ItemCount, DateTime CreatedAt);

/// <summary>List widget row: one line item of the oldest purchase orders.</summary>
public record OldestItemDto(int PurchaseOrderId, string PoNumber, string ItemName, decimal Quantity, DateTime CreatedAt);

/// <summary>Donut widget slice: total quantity for one item name.</summary>
public record ItemQuantityDto(string ItemName, decimal TotalQuantity, decimal Percent);

public record ItemsByQuantityResponse(decimal TotalQuantity, IReadOnlyList<ItemQuantityDto> Items);
