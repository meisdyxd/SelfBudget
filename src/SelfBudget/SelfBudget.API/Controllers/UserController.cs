using Microsoft.AspNetCore.Mvc;
using SelfBudget.API.Dtos.Requests;
using SelfBudget.API.UseCases.CreateUser;
using Wolverine;

namespace SelfBudget.API.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok();
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserRequest request,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        var command = CreateUserCommand.FromRequest(request);

        var result = await messageBus.InvokeAsync<Guid>(command, cancellationToken);

        return Ok(result);
    }
}
