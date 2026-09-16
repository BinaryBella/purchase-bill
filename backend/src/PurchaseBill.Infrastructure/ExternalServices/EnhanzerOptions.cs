namespace PurchaseBill.Infrastructure.ExternalServices;

public class EnhanzerOptions
{
    public const string SectionName = "Enhanzer";

    public required string BaseUrl { get; init; }
    public string DeviceId { get; init; } = "D001";
}
