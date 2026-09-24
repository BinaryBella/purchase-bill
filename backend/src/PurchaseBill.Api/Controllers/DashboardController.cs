using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PurchaseBill.Application.Dtos.Dashboard;
using PurchaseBill.Application.Services;

namespace PurchaseBill.Api.Controllers;

/// <summary>Data for the three widgets on the welcome dashboard.</summary>
[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    /// <summary>Table widget: the 5 most recently added purchase orders.</summary>
    [HttpGet("latest-orders")]
    [ProducesResponseType(typeof(IReadOnlyList<LatestOrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LatestOrderDto>>> GetLatestOrders(CancellationToken ct) =>
        Ok(await dashboardService.GetLatestOrdersAsync(ct));

    /// <summary>List widget: the 10 oldest purchase order line items.</summary>
    [HttpGet("oldest-items")]
    [ProducesResponseType(typeof(IReadOnlyList<OldestItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OldestItemDto>>> GetOldestItems(CancellationToken ct) =>
        Ok(await dashboardService.GetOldestItemsAsync(ct));

    /// <summary>Donut widget: total quantity grouped by item name.</summary>
    [HttpGet("items-by-quantity")]
    [ProducesResponseType(typeof(ItemsByQuantityResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ItemsByQuantityResponse>> GetItemsByQuantity(
        [FromQuery] DashboardRange range = DashboardRange.Today, CancellationToken ct = default) =>
        Ok(await dashboardService.GetItemsByQuantityAsync(range, ct));
}
