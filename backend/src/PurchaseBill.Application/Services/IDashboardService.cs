using PurchaseBill.Application.Dtos.Dashboard;

namespace PurchaseBill.Application.Services;

public interface IDashboardService
{
    Task<IReadOnlyList<LatestOrderDto>> GetLatestOrdersAsync(CancellationToken ct = default);
    Task<IReadOnlyList<OldestItemDto>> GetOldestItemsAsync(CancellationToken ct = default);
    Task<ItemsByQuantityResponse> GetItemsByQuantityAsync(DashboardRange range, CancellationToken ct = default);
}
