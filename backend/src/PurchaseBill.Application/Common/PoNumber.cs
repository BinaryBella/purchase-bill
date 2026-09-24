namespace PurchaseBill.Application.Common;

/// <summary>Formats the human-readable purchase order number shown in the UI (e.g. PO-000012).</summary>
public static class PoNumber
{
    public static string Format(int id) => $"PO-{id:D6}";

    /// <summary>Unique placeholder used only between the insert and the follow-up update that knows the Id.</summary>
    public static string Temporary() => $"TMP-{Guid.NewGuid():N}";
}
