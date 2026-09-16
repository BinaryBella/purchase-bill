namespace PurchaseBill.Application.Entities;

/// <summary>
/// Locations returned by the Enhanzer POS login API (User_Locations), persisted so the
/// Purchase Bill "Batch" dropdown can be populated without another external call.
/// </summary>
public class LocationDetail
{
    public int Id { get; set; }
    public string LocationCode { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
