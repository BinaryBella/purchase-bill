using PurchaseBill.Application.Dtos.Dashboard;
using PurchaseBill.Application.Entities;
using PurchaseBill.Application.Services;
using PurchaseBill.Infrastructure.Persistence;
using PurchaseBill.Tests.TestUtils;
using Xunit;

namespace PurchaseBill.Tests.Services;

public class DashboardServiceTests
{
    private static readonly DateTime Now = new(2026, 9, 24, 12, 0, 0, DateTimeKind.Utc);

    private sealed class FixedClock(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }

    private static DashboardService CreateService(ApplicationDbContext db) => new(db, new FixedClock(Now));

    private static async Task<PurchaseBill.Application.Entities.PurchaseBill> AddBill(
        ApplicationDbContext db, DateTime createdAt, decimal totalCost, params (string Name, decimal Qty)[] items)
    {
        var bill = new PurchaseBill.Application.Entities.PurchaseBill
        {
            CreatedByUsername = "tester",
            CreatedAt = createdAt,
            TotalItems = items.Length,
            TotalQuantity = items.Sum(i => i.Qty),
            TotalCost = totalCost,
            Items = items.Select(i => new PurchaseBillItem
            {
                ItemName = i.Name,
                BatchLocationCode = "L",
                BatchLocationName = "L",
                Quantity = i.Qty
            }).ToList()
        };
        db.PurchaseBills.Add(bill);
        await db.SaveChangesAsync();
        bill.PoNumber = $"PO-{bill.Id:D6}";
        await db.SaveChangesAsync();
        return bill;
    }

    [Fact]
    public async Task LatestOrders_ReturnsFiveNewestFirst_WithNetAmountAndItemCount()
    {
        var db = TestDbContextFactory.Create();
        for (var i = 0; i < 7; i++)
        {
            await AddBill(db, Now.AddMinutes(-60 + i), 100 + i, ("Mango", 1), ("Apple", 2));
        }

        var result = await CreateService(db).GetLatestOrdersAsync();

        Assert.Equal(5, result.Count);
        Assert.Equal(106m, result[0].NetAmount);
        Assert.Equal(2, result[0].ItemCount);
        Assert.Equal(result.OrderByDescending(r => r.CreatedAt).Select(r => r.Id), result.Select(r => r.Id));
        Assert.StartsWith("PO-", result[0].PoNumber);
    }

    [Fact]
    public async Task LatestOrders_IncludesBillsFromPreviousDays()
    {
        var db = TestDbContextFactory.Create();
        var old = await AddBill(db, Now.AddDays(-40), 50, ("Mango", 1));
        var recent = await AddBill(db, Now.AddHours(-2), 75, ("Apple", 1));

        var result = await CreateService(db).GetLatestOrdersAsync();

        Assert.Equal([recent.Id, old.Id], result.Select(r => r.Id));
    }

    [Fact]
    public async Task LatestOrders_WithNoBills_ReturnsEmpty()
    {
        var db = TestDbContextFactory.Create();

        Assert.Empty(await CreateService(db).GetLatestOrdersAsync());
    }

    [Fact]
    public async Task OldestItems_ReturnsTenOldestItemsFirst()
    {
        var db = TestDbContextFactory.Create();
        var first = await AddBill(db, Now.AddDays(-10), 10, ("Mango", 1), ("Apple", 2), ("Grapes", 3));
        for (var i = 0; i < 5; i++)
        {
            await AddBill(db, Now.AddDays(-9 + i), 10, ("Mango", 4), ("Apple", 5));
        }

        var result = await CreateService(db).GetOldestItemsAsync();

        Assert.Equal(10, result.Count);
        Assert.All(result.Take(3), r => Assert.Equal(first.Id, r.PurchaseOrderId));
        Assert.Equal(["Mango", "Apple", "Grapes"], result.Take(3).Select(r => r.ItemName));
        Assert.Equal(3m, result[2].Quantity);
        Assert.Equal(result.OrderBy(r => r.CreatedAt).Select(r => r.CreatedAt), result.Select(r => r.CreatedAt));
    }

    [Fact]
    public async Task ItemsByQuantity_GroupsByNameAndComputesPercent()
    {
        var db = TestDbContextFactory.Create();
        await AddBill(db, Now.AddHours(-1), 10, ("Mango", 5), ("Apple", 3));
        await AddBill(db, Now.AddHours(-2), 10, ("Mango", 2), ("Grapes", 10));

        var result = await CreateService(db).GetItemsByQuantityAsync(DashboardRange.All);

        Assert.Equal(20m, result.TotalQuantity);
        Assert.Equal(["Grapes", "Mango", "Apple"], result.Items.Select(i => i.ItemName));
        Assert.Equal(7m, result.Items.Single(i => i.ItemName == "Mango").TotalQuantity);
        Assert.Equal(50m, result.Items.Single(i => i.ItemName == "Grapes").Percent);
        Assert.Equal(15m, result.Items.Single(i => i.ItemName == "Apple").Percent);
    }

    [Fact]
    public async Task ItemsByQuantity_WithNoData_ReturnsZeroTotalAndNoSlices()
    {
        var db = TestDbContextFactory.Create();

        var result = await CreateService(db).GetItemsByQuantityAsync(DashboardRange.Today);

        Assert.Equal(0m, result.TotalQuantity);
        Assert.Empty(result.Items);
    }

    [Fact]
    public void StartUtc_ComputesInclusiveWindow()
    {
        Assert.Equal(new DateTime(2026, 9, 24), DashboardRange.Today.StartUtc(Now));
        Assert.Equal(new DateTime(2026, 9, 18), DashboardRange.Last7Days.StartUtc(Now));
        Assert.Equal(new DateTime(2026, 8, 26), DashboardRange.Last30Days.StartUtc(Now));
        Assert.Null(DashboardRange.All.StartUtc(Now));
    }
}
