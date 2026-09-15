using Microsoft.AspNetCore.Mvc;

namespace ProductCatalogApi;

public static class ResultExtensions
{
    public static ActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result)
    {
        return result.ErrorType switch
        {
            ErrorType.NotFound => controller.NotFound(result.ErrorMessage),
            ErrorType.Validation => controller.BadRequest(result.ErrorMessage),
            ErrorType.Conflict => controller.Conflict(result.ErrorMessage),
            ErrorType.UnAuthorized => controller.Unauthorized(result.ErrorMessage),
            ErrorType.Forbidden => controller.StatusCode(StatusCodes.Status403Forbidden, result.ErrorMessage),
            _ => controller.StatusCode(500, "An unexpected error occurred."),
        };
    }
}