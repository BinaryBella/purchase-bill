using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PurchaseBill.Application.Common;

namespace PurchaseBill.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/items")]
public class ItemsController : ControllerBase
{
    /// <summary>Feeds the Purchase Bill form's "Item" autocomplete with the fixed catalog list.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<string>> GetAll() => Ok(CatalogItems.Names);
}
