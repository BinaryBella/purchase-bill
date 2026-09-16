namespace PurchaseBill.Application.Entities;

/// <summary>
/// Header record for a submitted purchase bill.
/// </summary>
public class PurchaseBill
{
    public int Id { get; set; }
    public string CreatedByUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public int TotalItems { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalSelling { get; set; }

    public ICollection<PurchaseBillItem> Items { get; set; } = new List<PurchaseBillItem>();
}
