using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PurchaseBill.Api.Common;
using PurchaseBill.Application.Dtos.PurchaseBills;
using PurchaseBill.Application.Exceptions;
using PurchaseBill.Application.Services;

namespace PurchaseBill.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/purchase-bills")]
public class PurchaseBillsController(
    IPurchaseBillService purchaseBillService,
    IValidator<CreatePurchaseBillRequest> validator,
    ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Task 2: saves the rows built up in the Items grid as one Purchase Bill, returning the
    /// persisted totals (Total Items / Total Quantity) for the Item Summary panel.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PurchaseBillResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PurchaseBillResponse>> Create(CreatePurchaseBillRequest request, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            throw new ValidationAppException(validationResult.ToDictionary());
        }

        var response = await purchaseBillService.CreateAsync(currentUserService.Username, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>Lists saved purchase bills, newest first.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PurchaseBillSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PurchaseBillSummaryDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
        Ok(await purchaseBillService.GetPagedAsync(page, pageSize, ct));

    /// <summary>Returns one saved purchase bill with its line items.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PurchaseBillResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PurchaseBillResponse>> GetById(int id, CancellationToken ct) =>
        Ok(await purchaseBillService.GetByIdAsync(id, ct));
}
