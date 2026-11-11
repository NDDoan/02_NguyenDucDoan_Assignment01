using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.Filters;

/// <summary>
/// Automatically validates ModelState for all controller actions (including ODataController)
/// and returns a standardized 400 Bad Request JSON response with response headers.
/// </summary>
public class ValidateModelFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            context.Result = CreateValidationErrorResult(context.ModelState, context.HttpContext.Response);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }

    public static BadRequestObjectResult CreateValidationErrorResult(
        ModelStateDictionary modelState,
        HttpResponse response)
    {
        var errors = modelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        var firstMessage = errors.Values
            .SelectMany(messages => messages)
            .FirstOrDefault(msg => !string.IsNullOrWhiteSpace(msg))
            ?? "Dữ liệu đầu vào không hợp lệ.";

        response.Headers.Append("X-Validation-Error", "ModelStateInvalid");

        return new BadRequestObjectResult(new
        {
            statusCode = StatusCodes.Status400BadRequest,
            error = "Bad Request",
            message = firstMessage,
            errors
        });
    }
}

