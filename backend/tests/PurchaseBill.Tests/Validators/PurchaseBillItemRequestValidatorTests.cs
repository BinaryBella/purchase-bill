using PurchaseBill.Application.Dtos.PurchaseBills;
using PurchaseBill.Application.Validators;
using Xunit;

namespace PurchaseBill.Tests.Validators;

public class PurchaseBillItemRequestValidatorTests
{
    private readonly PurchaseBillItemRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidItem_Passes()
    {
        var result = _validator.Validate(new PurchaseBillItemRequest("Mango", "LOC-1", 100, 150, 5, 1, 20));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithItemNotInCatalog_Fails()
    {
        var result = _validator.Validate(new PurchaseBillItemRequest("Watermelon", "LOC-1", 100, 150, 5, 0, 20));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PurchaseBillItemRequest.ItemName));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithNonPositiveQuantity_Fails(decimal quantity)
    {
        var result = _validator.Validate(new PurchaseBillItemRequest("Mango", "LOC-1", 100, 150, quantity, 0, 20));

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Validate_WithDiscountOutOfRange_Fails(decimal discount)
    {
        var result = _validator.Validate(new PurchaseBillItemRequest("Mango", "LOC-1", 100, 150, 5, 0, discount));

        Assert.False(result.IsValid);
    }
}
