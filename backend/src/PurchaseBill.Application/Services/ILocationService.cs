using PurchaseBill.Application.Dtos.Auth;

namespace PurchaseBill.Application.Services;

public interface ILocationService
{
    Task<IReadOnlyList<LocationDto>> GetAllAsync(CancellationToken ct = default);
}
