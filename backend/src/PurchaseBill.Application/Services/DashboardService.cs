using Microsoft.EntityFrameworkCore;
using PurchaseBill.Application.Dtos.Dashboard;
using PurchaseBill.Application.Interfaces;

namespace PurchaseBill.Application.Services;

/// <summary>
/// Read-only queries behind the three welcome-dashboard widgets, projected straight to DTOs
/// (no entity tracking). The latest-orders and oldest-items widgets always cover every bill;
/// only the items-by-quantity widget takes a date range, which is evaluated against UTC since
/// bills are stamped with <c>DateTime.UtcNow</c>.
/// </summary>
public class DashboardService(IApplicationDbContext db, TimeProvider clock) : IDashboardService
{
    private const int LatestOrderCount = 5;
    private const int OldestItemCount = 10;

    public async Task<IReadOnlyList<LatestOrderDto>> GetLatestOrdersAsync(CancellationToken ct = default)
    {
        return await db.PurchaseBills
            .AsNoTracking()
            .OrderByDescending(b => b.CreatedAt).ThenByDescending(b => b.Id)
            .Take(LatestOrderCount)
            .Select(b => new LatestOrderDto(b.Id, b.PoNumber, b.TotalCost, b.TotalItems, b.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<OldestItemDto>> GetOldestItemsAsync(CancellationToken ct = default)
    {
        return await db.PurchaseBillItems
            .AsNoTracking()
            .OrderBy(i => i.PurchaseBill!.CreatedAt).ThenBy(i => i.PurchaseBillId).ThenBy(i => i.Id)
            .Take(OldestItemCount)
            .Select(i => new OldestItemDto(
                i.PurchaseBillId, i.PurchaseBill!.PoNumber, i.ItemName, i.Quantity, i.PurchaseBill.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<ItemsByQuantityResponse> GetItemsByQuantityAsync(DashboardRange range, CancellationToken ct = default)
    {
        var start = range.StartUtc(clock.GetUtcNow().UtcDateTime);

        var groups = await db.PurchaseBillItems
            .AsNoTracking()
            .Where(i => start == null || i.PurchaseBill!.CreatedAt >= start)
            .GroupBy(i => i.ItemName)
            .Select(g => new { ItemName = g.Key, TotalQuantity = g.Sum(i => i.Quantity) })
            .ToListAsync(ct);

        var total = groups.Sum(g => g.TotalQuantity);
        var items = groups
            .OrderByDescending(g => g.TotalQuantity).ThenBy(g => g.ItemName)
            .Select(g => new ItemQuantityDto(
                g.ItemName, g.TotalQuantity, total == 0 ? 0 : Math.Round(g.TotalQuantity / total * 100, 1)))
            .ToList();

        return new ItemsByQuantityResponse(total, items);
    }
}
