using Microsoft.EntityFrameworkCore;
using PurchaseBill.Infrastructure.Persistence;

namespace PurchaseBill.Tests.TestUtils;

/// <summary>Builds an isolated in-memory ApplicationDbContext, one per test, so tests never share state.</summary>
public static class TestDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
