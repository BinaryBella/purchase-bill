using PurchaseBill.Application.Dtos.Enhanzer;

namespace PurchaseBill.Application.Interfaces;

/// <summary>Talks to the external Enhanzer POS_Api/Invoke login endpoint.</summary>
public interface IEnhanzerAuthClient
{
    Task<EnhanzerApiEnvelope> GetLoginDataAsync(string email, string password, CancellationToken ct = default);
}
