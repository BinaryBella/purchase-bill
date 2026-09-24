using PurchaseBill.Application.Dtos.PurchaseBills;

namespace PurchaseBill.Application.Services;

public interface IPurchaseBillService
{
    Task<PurchaseBillResponse> CreateAsync(string username, CreatePurchaseBillRequest request, CancellationToken ct = default);
    Task<PagedResult<PurchaseBillSummaryDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<PurchaseBillResponse> GetByIdAsync(int id, CancellationToken ct = default);
}
