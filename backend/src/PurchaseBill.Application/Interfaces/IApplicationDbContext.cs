using Microsoft.EntityFrameworkCore;
using PurchaseBill.Application.Entities;

namespace PurchaseBill.Application.Interfaces;

/// <summary>
/// Persistence seam the Application layer depends on, so its services stay ignorant of EF Core;
/// PurchaseBill.Infrastructure provides the real DbContext-backed implementation.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<LocationDetail> LocationDetails { get; }
    DbSet<Entities.PurchaseBill> PurchaseBills { get; }
    DbSet<PurchaseBillItem> PurchaseBillItems { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
