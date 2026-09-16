using Microsoft.EntityFrameworkCore;
using PurchaseBill.Application.Common;
using PurchaseBill.Application.Dtos.PurchaseBills;
using PurchaseBill.Application.Entities;
using PurchaseBill.Application.Exceptions;
using PurchaseBill.Application.Interfaces;

namespace PurchaseBill.Application.Services;

/// <summary>
/// Drives Task 2's "Add" + submit flow: resolves each row's batch against Location_Details,
/// computes Margin/Total Cost/Total Selling server-side (the authoritative values - the
/// Angular UI mirrors the same formulas only for instant feedback), and persists the bill with
/// its header summary (Total Items / Total Quantity).
/// </summary>
public class PurchaseBillService(IApplicationDbContext db) : IPurchaseBillService
{
    public async Task<PurchaseBillResponse> CreateAsync(string username, CreatePurchaseBillRequest request, CancellationToken ct = default)
    {
        var locationCodes = request.Items.Select(i => i.BatchLocationCode).Distinct().ToList();
        var locations = await db.LocationDetails
            .Where(l => locationCodes.Contains(l.LocationCode))
            .ToDictionaryAsync(l => l.LocationCode, ct);

        var missing = locationCodes.Where(code => !locations.ContainsKey(code)).ToList();
        if (missing.Count > 0)
        {
            throw new EntityNotFoundException($"Unknown batch location(s): {string.Join(", ", missing)}.");
        }

        var bill = new Entities.PurchaseBill
        {
            CreatedByUsername = username,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in request.Items)
        {
            var location = locations[item.BatchLocationCode];
            bill.Items.Add(new PurchaseBillItem
            {
                ItemName = item.ItemName,
                BatchLocationCode = location.LocationCode,
                BatchLocationName = location.LocationName,
                StandardCost = item.StandardCost,
                StandardPrice = item.StandardPrice,
                Margin = PurchaseBillCalculator.Margin(item.StandardCost, item.StandardPrice),
                Quantity = item.Quantity,
                FreeQuantity = item.FreeQuantity,
                DiscountPercent = item.DiscountPercent,
                TotalCost = PurchaseBillCalculator.TotalCost(item.StandardCost, item.Quantity, item.DiscountPercent),
                TotalSelling = PurchaseBillCalculator.TotalSelling(item.StandardPrice, item.Quantity)
            });
        }

        bill.TotalItems = bill.Items.Count;
        bill.TotalQuantity = bill.Items.Sum(i => i.Quantity);
        bill.TotalCost = bill.Items.Sum(i => i.TotalCost);
        bill.TotalSelling = bill.Items.Sum(i => i.TotalSelling);

        db.PurchaseBills.Add(bill);
        await db.SaveChangesAsync(ct);

        return new PurchaseBillResponse(
            bill.Id,
            bill.CreatedAt,
            bill.TotalItems,
            bill.TotalQuantity,
            bill.TotalCost,
            bill.TotalSelling,
            bill.Items.Select(i => new PurchaseBillItemResponse(
                i.Id, i.ItemName, i.BatchLocationCode, i.BatchLocationName,
                i.StandardCost, i.StandardPrice, i.Margin, i.Quantity, i.FreeQuantity,
                i.DiscountPercent, i.TotalCost, i.TotalSelling)).ToList());
    }
}
