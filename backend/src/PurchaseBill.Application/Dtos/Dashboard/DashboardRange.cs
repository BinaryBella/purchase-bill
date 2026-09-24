namespace PurchaseBill.Application.Dtos.Dashboard;

/// <summary>The date window behind each widget's "Today" dropdown.</summary>
public enum DashboardRange
{
    Today,
    Last7Days,
    Last30Days,
    All
}

public static class DashboardRangeExtensions
{
    /// <summary>Inclusive UTC lower bound for the range, or null when the range is unbounded.</summary>
    public static DateTime? StartUtc(this DashboardRange range, DateTime utcNow) => range switch
    {
        DashboardRange.Today => utcNow.Date,
        DashboardRange.Last7Days => utcNow.Date.AddDays(-6),
        DashboardRange.Last30Days => utcNow.Date.AddDays(-29),
        _ => null
    };
}
