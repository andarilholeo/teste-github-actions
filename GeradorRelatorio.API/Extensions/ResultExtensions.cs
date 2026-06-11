using Microsoft.AspNetCore.Mvc;
using GeradorRelatorio.Application.Common;

namespace GeradorRelatorio.API.Extensions;

public static class ResultExtensions
{
    public static ActionResult<T> ToActionResult<T>(this Result<T> result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(result.Value);
        }

        var error = result.Error!;
        var status = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status422UnprocessableEntity,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.External => StatusCodes.Status502BadGateway,
            _ => StatusCodes.Status500InternalServerError
        };

        return controller.Problem(detail: error.Message, statusCode: status);
    }
}
