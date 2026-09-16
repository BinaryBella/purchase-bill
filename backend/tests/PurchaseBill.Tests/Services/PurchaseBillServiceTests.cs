using PurchaseBill.Application.Dtos.PurchaseBills;
using PurchaseBill.Application.Entities;
using PurchaseBill.Application.Exceptions;
using PurchaseBill.Application.Services;
using PurchaseBill.Tests.TestUtils;
using Xunit;

namespace PurchaseBill.Tests.Services;

public class PurchaseBillServiceTests
{
    [Fact]
    public async Task CreateAsync_ComputesTotalsAndSummary_MatchingBriefExample()
    {
        var db = TestDbContextFactory.Create();
        db.LocationDetails.Add(new LocationDetail { LocationCode = "LOC-1", LocationName = "Head Office", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var service = new PurchaseBillService(db);

        var request = new CreatePurchaseBillRequest([
            new PurchaseBillItemRequest("Mango", "LOC-1", StandardCost: 100, StandardPrice: 150, Quantity: 5, FreeQuantity: 1, DiscountPercent: 20)
        ]);

        var response = await service.CreateAsync("info@enhanzer.com", request);

        Assert.Equal(1, response.TotalItems);
        Assert.Equal(5, response.TotalQuantity);
        Assert.Equal(400m, response.TotalCost);
        Assert.Equal(750m, response.TotalSelling);
        Assert.Equal("Head Office", response.Items.Single().BatchLocationName);
    }

    [Fact]
    public async Task CreateAsync_WithMultipleRows_SummarizesTotalItemsAndTotalQuantity()
    {
        var db = TestDbContextFactory.Create();
        db.LocationDetails.Add(new LocationDetail { LocationCode = "LOC-1", LocationName = "Head Office", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var service = new PurchaseBillService(db);

        var request = new CreatePurchaseBillRequest([
            new PurchaseBillItemRequest("Mango", "LOC-1", 100, 150, 5, 0, 20),
            new PurchaseBillItemRequest("Apple", "LOC-1", 50, 80, 3, 1, 0)
        ]);

        var response = await service.CreateAsync("info@enhanzer.com", request);

        Assert.Equal(2, response.TotalItems);
        Assert.Equal(8, response.TotalQuantity);
    }

    [Fact]
    public async Task CreateAsync_WithUnknownBatchCode_ThrowsNotFound()
    {
        var db = TestDbContextFactory.Create();
        var service = new PurchaseBillService(db);

        var request = new CreatePurchaseBillRequest([
            new PurchaseBillItemRequest("Mango", "DOES-NOT-EXIST", 100, 150, 5, 0, 20)
        ]);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => service.CreateAsync("info@enhanzer.com", request));
    }
}
