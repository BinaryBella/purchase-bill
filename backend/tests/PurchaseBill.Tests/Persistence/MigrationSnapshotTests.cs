using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using PurchaseBill.Infrastructure.Persistence;
using Xunit;

namespace PurchaseBill.Tests.Persistence;

public class MigrationSnapshotTests
{
    /// <summary>Fails if an entity/configuration change was made without adding a migration.</summary>
    [Fact]
    public void ModelSnapshot_MatchesCurrentModel()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(local);Database=unused;Trusted_Connection=True;")
            .Options;
        using var db = new ApplicationDbContext(options);

        var snapshotModel = db.GetService<IMigrationsAssembly>().ModelSnapshot!.Model;
        var designTimeModel = db.GetService<IDesignTimeModel>().Model;
        var differ = db.GetService<IMigrationsModelDiffer>();

        var snapshotRelational = db.GetService<IModelRuntimeInitializer>()
            .Initialize(snapshotModel).GetRelationalModel();
        var differences = differ.GetDifferences(snapshotRelational, designTimeModel.GetRelationalModel());

        Assert.Empty(differences);
    }
}
