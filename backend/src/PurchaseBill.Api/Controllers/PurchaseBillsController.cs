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
        return CreatedAtAction(nameof(Create), new { id = response.Id }, response);
    }
}
