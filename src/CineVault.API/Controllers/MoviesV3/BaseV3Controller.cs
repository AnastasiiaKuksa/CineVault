using CineVault.API.Common.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CineVault.API.Controllers.MoviesV3;

[ApiController]
public abstract class BaseV3Controller : ControllerBase
{
    protected ActionResult<ApiResponse<T>> Ok<T>(T data, string requestId, string message = "Success")
        => base.Ok(new ApiResponse<T>
        {
            Success = true,
            Message = message,
            RequestId = requestId,
            ApiVersion = "v3",
            Data = data
        });

    protected ActionResult<ApiResponse<T>> Created<T>(T data, string requestId, string message = "Created")
        => StatusCode(201, new ApiResponse<T>
        {
            Success = true,
            Message = message,
            RequestId = requestId,
            ApiVersion = "v3",
            Data = data
        });

    protected ActionResult<ApiResponse<object>> BadRequest(string requestId, string message = "Bad Request")
        => base.BadRequest(new ApiResponse<object>
        {
            Success = false,
            Message = message,
            RequestId = requestId,
            ApiVersion = "v3"
        });

    protected ActionResult<ApiResponse<object>> NotFound(string requestId, string message = "Not Found")
        => base.NotFound(new ApiResponse<object>
        {
            Success = false,
            Message = message,
            RequestId = requestId,
            ApiVersion = "v3"
        });
}