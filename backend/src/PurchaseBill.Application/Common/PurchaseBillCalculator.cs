namespace PurchaseBill.Application.Common;

/// <summary>
/// Line-item calculations for the Purchase Bill form, per the assignment brief:
///   Total Cost    = Standard Cost * Quantity * (1 - Discount%)
///   Total Selling = Standard Price * Quantity
///   Margin        = Standard Price - Standard Cost
///
/// Kept here (rather than inline in the service) so the exact same rule can be unit tested and
/// referenced from one place; the Angular side mirrors this for live, pre-submit totals.
/// </summary>
public static class PurchaseBillCalculator
{
    public static decimal Margin(decimal standardCost, decimal standardPrice) =>
        Round(standardPrice - standardCost);

    public static decimal TotalCost(decimal standardCost, decimal quantity, decimal discountPercent) =>
        Round(standardCost * quantity * (1 - discountPercent / 100m));

    public static decimal TotalSelling(decimal standardPrice, decimal quantity) =>
        Round(standardPrice * quantity);

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
