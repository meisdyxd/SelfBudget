using Microsoft.AspNetCore.Mvc;

namespace SelfBudget.API.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok();
    }
}
