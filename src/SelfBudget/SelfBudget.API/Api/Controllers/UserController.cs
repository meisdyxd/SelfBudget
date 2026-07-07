using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using SelfBudget.API.Application.UseCases.UserUseCases.CreateUser;
using SelfBudget.API.Common;
using SelfBudget.API.Common.Dtos.Requests.UserRequests;
using Wolverine;

namespace SelfBudget.API.Api.Controllers;

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
