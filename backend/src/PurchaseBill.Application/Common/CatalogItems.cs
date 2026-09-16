namespace PurchaseBill.Application.Common;

/// <summary>The fixed item list the "Item" autocomplete is populated with, per the brief.</summary>
public static class CatalogItems
{
    public static readonly IReadOnlyList<string> Names =
        ["Mango", "Apple", "Banana", "Orange", "Grapes", "Kiwi", "Strawberry"];
}
