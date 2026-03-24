using Microsoft.AspNetCore.Mvc;

namespace CineVault.API.Controllers;

[ApiController]
[Route("api")]
public class AppInfoController : ControllerBase
{
    private readonly IHostEnvironment _environment;

    public AppInfoController(IHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpGet("environment")]
    public IActionResult GetEnvironment()
    {
        return Ok(new
        {
            Environment = _environment.EnvironmentName,
            // Додаткова логіка для перевірки
            IsDevelopment = _environment.IsDevelopment(),
            IsLocal = _environment.IsEnvironment("Local")
        });
    }
}