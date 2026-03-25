using Asp.Versioning;
using CineVault.API.Common.Requests;
using CineVault.API.Common.Responses;
using CineVault.API.Controllers.MoviesV3;
using Microsoft.AspNetCore.Mvc;

namespace CineVault.API.Controllers;

[ApiVersion(1.0, Deprecated = true)]
[ApiVersion(2.0)]
[ApiVersion(3.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public class AppInfoController : BaseV3Controller
{
    private readonly IHostEnvironment _environment;

    public AppInfoController(IHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpGet("environment"), MapToApiVersion(1.0)]
    public IActionResult GetEnvironmentV1()
    {
        return base.Ok(_environment.EnvironmentName);
    }

    [HttpGet("environment"), MapToApiVersion(2.0)]
    public IActionResult GetEnvironmentV2()
    {
        return base.Ok(new
        {
            Version = "v2",
            Environment = _environment.EnvironmentName,
            MachineName = Environment.MachineName,
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpPost("environment"), MapToApiVersion(3.0)]
    public IActionResult GetEnvironmentV3([FromBody] ApiRequest request)
    {
        var data = new
        {
            Environment = _environment.EnvironmentName,
            MachineName = Environment.MachineName,
            Timestamp = DateTime.UtcNow,
            RequestId = request.RequestId
        };
        return base.Ok(ApiResponse<object>.Ok(request.RequestId, data, "Environment info retrieved"));
    }
}