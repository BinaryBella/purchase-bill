using PurchaseBill.Application.Dtos.PurchaseBills;

namespace PurchaseBill.Application.Services;

public interface IPurchaseBillService
{
    Task<PurchaseBillResponse> CreateAsync(string username, CreatePurchaseBillRequest request, CancellationToken ct = default);
}
