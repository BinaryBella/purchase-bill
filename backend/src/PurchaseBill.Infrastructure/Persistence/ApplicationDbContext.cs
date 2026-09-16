using Microsoft.EntityFrameworkCore;
using PurchaseBill.Application.Entities;
using PurchaseBill.Application.Interfaces;

namespace PurchaseBill.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<LocationDetail> LocationDetails => Set<LocationDetail>();
    public DbSet<Application.Entities.PurchaseBill> PurchaseBills => Set<Application.Entities.PurchaseBill>();
    public DbSet<PurchaseBillItem> PurchaseBillItems => Set<PurchaseBillItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
