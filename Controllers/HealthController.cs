using Microsoft.AspNetCore.Mvc;

namespace LiteFactoryApi.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "ok",
            service = "LiteFactoryApi",
            timeUtc = DateTimeOffset.UtcNow
        });
    }
}
