using Microsoft.AspNetCore.Mvc;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result) =>
        result.IsSuccess
            ? new OkResult()
            : new ObjectResult(new ProblemDetails { Title = result.Error.Code, Detail = result.Error.Message })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };

    public static IActionResult ToActionResult<TValue>(this Result<TValue> result) =>
        result.IsSuccess
            ? new OkObjectResult(result.Value)
            : new ObjectResult(new ProblemDetails { Title = result.Error.Code, Detail = result.Error.Message })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
}
