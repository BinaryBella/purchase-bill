using PurchaseBill.Application.Dtos.Auth;

namespace PurchaseBill.Application.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
