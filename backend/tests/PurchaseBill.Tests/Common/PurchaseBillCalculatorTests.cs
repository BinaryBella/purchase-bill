using PurchaseBill.Application.Common;
using Xunit;

namespace PurchaseBill.Tests.Common;

public class PurchaseBillCalculatorTests
{
    [Fact]
    public void TotalCost_MatchesBriefWorkedExample()
    {
        // Standard Cost = 100, Qty = 5, Discount = 20% -> Total Cost = 100 * 5 * (1 - 20%) = 400
        var result = PurchaseBillCalculator.TotalCost(standardCost: 100, quantity: 5, discountPercent: 20);

        Assert.Equal(400m, result);
    }

    [Fact]
    public void TotalSelling_MatchesBriefWorkedExample()
    {
        // Standard Price = 150, Qty = 5 -> Total Selling = 150 * 5 = 750
        var result = PurchaseBillCalculator.TotalSelling(standardPrice: 150, quantity: 5);

        Assert.Equal(750m, result);
    }

    [Fact]
    public void Margin_IsPriceMinusCost()
    {
        var result = PurchaseBillCalculator.Margin(standardCost: 100, standardPrice: 150);

        Assert.Equal(50m, result);
    }

    [Theory]
    [InlineData(0, 10, 0, 0)]
    [InlineData(100, 1, 0, 100)]
    public void TotalCost_HandlesZeroDiscountAndZeroCost(decimal cost, decimal qty, decimal discount, decimal expected)
    {
        var result = PurchaseBillCalculator.TotalCost(cost, qty, discount);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void TotalCost_FullDiscountZeroesOutTheCost()
    {
        var result = PurchaseBillCalculator.TotalCost(standardCost: 200, quantity: 3, discountPercent: 100);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void TotalCost_RoundsToTwoDecimalPlaces()
    {
        var result = PurchaseBillCalculator.TotalCost(standardCost: 33.333m, quantity: 3, discountPercent: 10);

        // 33.333 * 3 * 0.9 = 89.9991 -> rounds to 90.00
        Assert.Equal(90.00m, result);
    }
}
