using System.Net;
using Microsoft.AspNetCore.Mvc;
using PurchaseBill.Application.Exceptions;

namespace PurchaseBill.Api.Middleware;

/// <summary>
/// Central place that turns Application-layer exceptions into consistent ProblemDetails
/// responses, so controllers stay free of try/catch noise.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AuthenticationFailedException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.Unauthorized, "Authentication failed", ex.Message);
        }
        catch (EntityNotFoundException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.NotFound, "Not found", ex.Message);
        }
        catch (ValidationAppException ex)
        {
            var problem = new ValidationProblemDetails(ex.Errors.ToDictionary(kv => kv.Key, kv => kv.Value))
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Validation failed"
            };
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsJsonAsync(problem);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteProblemAsync(context, HttpStatusCode.InternalServerError, "Unexpected error", "An unexpected error occurred. Please try again.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, HttpStatusCode status, string title, string detail)
    {
        var problem = new ProblemDetails
        {
            Status = (int)status,
            Title = title,
            Detail = detail
        };
        context.Response.StatusCode = (int)status;
        await context.Response.WriteAsJsonAsync(problem);
    }
}
