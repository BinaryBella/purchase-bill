namespace PurchaseBill.Application.Dtos.PurchaseBills;

public record PurchaseBillSummaryDto(int Id, string PoNumber, DateTime CreatedAt, int TotalItems, decimal TotalQuantity, decimal NetAmount);

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
