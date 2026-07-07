using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using SelfBudget.API.Common;
using SelfBudget.API.Dtos.Requests.UserRequests;
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

        var result = await messageBus.InvokeAsync<Result<Guid, Error>>(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
