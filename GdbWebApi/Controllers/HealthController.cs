using Microsoft.AspNetCore.Mvc;

namespace GdbWebApi.Controllers;

/// <summary>
/// Provides health check endpoints for verifying API service availability.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Checks the health and availability of the GDB Web API service.
    /// </summary>
    /// <returns>A status message and current UTC timestamp.</returns>
    /// <response code="200">The service is healthy and operational.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Check()
    {
        return Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow
        });
    }
}
