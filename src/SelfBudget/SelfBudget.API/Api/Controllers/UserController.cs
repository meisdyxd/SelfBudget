using Microsoft.AspNetCore.Mvc;
using SelfBudget.API.Common.Dtos.UserDtos;

namespace SelfBudget.API.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok();
    }

    [HttpGet("{id:guid}", Name = "GetUserById")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
