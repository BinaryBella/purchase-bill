using PurchaseBill.Application.Entities;
using PurchaseBill.Application.Services;
using PurchaseBill.Tests.TestUtils;
using Xunit;

namespace PurchaseBill.Tests.Services;

public class LocationServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsLocationsOrderedByName()
    {
        var db = TestDbContextFactory.Create();
        db.LocationDetails.AddRange(
            new LocationDetail { LocationCode = "LOC-2", LocationName = "Warehouse", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new LocationDetail { LocationCode = "LOC-1", LocationName = "Head Office", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var service = new LocationService(db);

        var result = await service.GetAllAsync();

        Assert.Equal(["Head Office", "Warehouse"], result.Select(l => l.LocationName));
    }
}
