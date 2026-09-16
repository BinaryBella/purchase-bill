using Microsoft.EntityFrameworkCore;
using PurchaseBill.Application.Dtos.Auth;
using PurchaseBill.Application.Interfaces;

namespace PurchaseBill.Application.Services;

/// <summary>Feeds the "Batch" dropdown from the Location_Details table saved at login.</summary>
public class LocationService(IApplicationDbContext db) : ILocationService
{
    public async Task<IReadOnlyList<LocationDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.LocationDetails
            .OrderBy(l => l.LocationName)
            .Select(l => new LocationDto(l.LocationCode, l.LocationName))
            .ToListAsync(ct);
    }
}
