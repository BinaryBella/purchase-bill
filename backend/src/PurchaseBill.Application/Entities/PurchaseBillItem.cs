namespace PurchaseBill.Application.Entities;

/// <summary>
/// A single line item row on a purchase bill, matching the "Items" grid in the UI.
/// </summary>
public class PurchaseBillItem
{
    public int Id { get; set; }
    public int PurchaseBillId { get; set; }
    public PurchaseBill? PurchaseBill { get; set; }

    public string ItemName { get; set; } = string.Empty;
    public string BatchLocationCode { get; set; } = string.Empty;
    public string BatchLocationName { get; set; } = string.Empty;

    public decimal StandardCost { get; set; }
    public decimal StandardPrice { get; set; }
    public decimal Margin { get; set; }
    public decimal Quantity { get; set; }
    public decimal FreeQuantity { get; set; }
    public decimal DiscountPercent { get; set; }

    public decimal TotalCost { get; set; }
    public decimal TotalSelling { get; set; }
}
