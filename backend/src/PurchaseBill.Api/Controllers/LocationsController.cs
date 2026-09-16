using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PurchaseBill.Application.Dtos.Auth;
using PurchaseBill.Application.Services;

namespace PurchaseBill.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/locations")]
public class LocationsController(ILocationService locationService) : ControllerBase
{
    /// <summary>Feeds the Purchase Bill form's "Batch" dropdown from Location_Details.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<LocationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LocationDto>>> GetAll(CancellationToken ct)
    {
        var locations = await locationService.GetAllAsync(ct);
        return Ok(locations);
    }
}
