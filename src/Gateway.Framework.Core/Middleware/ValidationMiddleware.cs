using Gateway.Framework.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Gateway.Framework.Core.Middleware;

/// <summary>
/// Action filter that validates model state and returns standardized validation error responses.
/// </summary>
public class ValidationActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors.Select(err => new ApiError(
                    BankingErrorCodes.ValidationError,
                    err.ErrorMessage,
                    e.Key)))
                .ToList();

            var response = ApiResponse.ValidationFail(errors);
            context.Result = new ObjectResult(response) { StatusCode = 422 };
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
