using PurchaseBill.Application.Dtos.PurchaseBills;
using PurchaseBill.Application.Entities;
using PurchaseBill.Application.Exceptions;
using PurchaseBill.Application.Services;
using PurchaseBill.Infrastructure.Persistence;
using PurchaseBill.Tests.TestUtils;
using Xunit;

namespace PurchaseBill.Tests.Services;

public class PurchaseBillQueryTests
{
    private static async Task<(ApplicationDbContext Db, PurchaseBillService Service)> Setup()
    {
        var db = TestDbContextFactory.Create();
        db.LocationDetails.Add(new LocationDetail { LocationCode = "LOC-1", LocationName = "Head Office", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
        return (db, new PurchaseBillService(db));
    }

    private static CreatePurchaseBillRequest Request(string item = "Mango") =>
        new([new PurchaseBillItemRequest(item, "LOC-1", 100, 150, 5, 0, 20)]);

    [Fact]
    public async Task CreateAsync_AssignsPoNumberFromId()
    {
        var (db, service) = await Setup();

        var response = await service.CreateAsync("u", Request());

        Assert.Equal($"PO-{response.Id:D6}", response.PoNumber);
        Assert.Equal(response.PoNumber, db.PurchaseBills.Single().PoNumber);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsNewestFirstAndPaginates()
    {
        var (_, service) = await Setup();
        for (var i = 0; i < 3; i++)
        {
            await service.CreateAsync("u", Request());
            await Task.Delay(5);
        }

        var page1 = await service.GetPagedAsync(1, 2);
        var page2 = await service.GetPagedAsync(2, 2);

        Assert.Equal(3, page1.TotalCount);
        Assert.Equal(2, page1.Items.Count);
        Assert.Single(page2.Items);
        Assert.True(page1.Items[0].CreatedAt >= page1.Items[1].CreatedAt);
        Assert.Equal(400m, page1.Items[0].NetAmount);
    }

    [Fact]
    public async Task GetPagedAsync_ClampsInvalidPaging()
    {
        var (_, service) = await Setup();

        var result = await service.GetPagedAsync(-4, 5000);

        Assert.Equal(1, result.Page);
        Assert.Equal(100, result.PageSize);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsBillWithItems()
    {
        var (_, service) = await Setup();
        var created = await service.CreateAsync("u", Request("Apple"));

        var loaded = await service.GetByIdAsync(created.Id);

        Assert.Equal(created.PoNumber, loaded.PoNumber);
        Assert.Equal("Apple", Assert.Single(loaded.Items).ItemName);
    }

    [Fact]
    public async Task GetByIdAsync_WithUnknownId_ThrowsNotFound()
    {
        var (_, service) = await Setup();

        await Assert.ThrowsAsync<EntityNotFoundException>(() => service.GetByIdAsync(999));
    }
}
