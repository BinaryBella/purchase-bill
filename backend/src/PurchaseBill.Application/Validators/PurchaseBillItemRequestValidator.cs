using FluentValidation;
using PurchaseBill.Application.Common;
using PurchaseBill.Application.Dtos.PurchaseBills;

namespace PurchaseBill.Application.Validators;

public class PurchaseBillItemRequestValidator : AbstractValidator<PurchaseBillItemRequest>
{
    public PurchaseBillItemRequestValidator()
    {
        RuleFor(x => x.ItemName)
            .NotEmpty().WithMessage("Item is required.")
            .Must(name => CatalogItems.Names.Contains(name))
            .WithMessage($"Item must be one of: {string.Join(", ", CatalogItems.Names)}.");

        RuleFor(x => x.BatchLocationCode)
            .NotEmpty().WithMessage("Batch is required.");

        RuleFor(x => x.StandardCost).GreaterThanOrEqualTo(0).WithMessage("Standard Cost cannot be negative.");
        RuleFor(x => x.StandardPrice).GreaterThanOrEqualTo(0).WithMessage("Standard Price cannot be negative.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        RuleFor(x => x.FreeQuantity).GreaterThanOrEqualTo(0).WithMessage("Free Qty cannot be negative.");
        RuleFor(x => x.DiscountPercent).InclusiveBetween(0, 100).WithMessage("Discount must be between 0 and 100.");
    }
}

public class CreatePurchaseBillRequestValidator : AbstractValidator<CreatePurchaseBillRequest>
{
    public CreatePurchaseBillRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item is required.");

        RuleForEach(x => x.Items).SetValidator(new PurchaseBillItemRequestValidator());
    }
}
