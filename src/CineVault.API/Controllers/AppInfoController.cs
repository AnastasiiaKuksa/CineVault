using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace CineVault.API.Controllers;

[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AppInfoController : ControllerBase
{
    private readonly IHostEnvironment _environment;

    public AppInfoController(IHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpGet("environment"), MapToApiVersion(1.0)]
    public IActionResult GetEnvironmentV1()
    {
        return Ok(_environment.EnvironmentName);
    }

    [HttpGet("environment"), MapToApiVersion(2.0)]
    public IActionResult GetEnvironmentV2()
    {
        return Ok(new
        {
            Version = "v2",
            Environment = _environment.EnvironmentName,
            MachineName = Environment.MachineName,
            Timestamp = DateTime.UtcNow
        });
    }
}