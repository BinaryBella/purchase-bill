using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PurchaseBill.Application.Dtos.Auth;
using PurchaseBill.Application.Exceptions;
using PurchaseBill.Application.Services;

namespace PurchaseBill.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService, IValidator<LoginRequest> validator) : ControllerBase
{
    /// <summary>
    /// Task 1: authenticates against the Enhanzer POS API, saves User_Locations into
    /// Location_Details, and returns our own session token for subsequent calls.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            throw new ValidationAppException(validationResult.ToDictionary());
        }

        var response = await authService.LoginAsync(request, ct);
        return Ok(response);
    }
}
