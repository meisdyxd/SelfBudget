using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using SelfBudget.API.Application.UseCases.UserUseCases.GetUserById;
using SelfBudget.API.Common;
using SelfBudget.API.Common.Dtos.UserDtos;
using Wolverine;

namespace SelfBudget.API.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMessageBus _messageBus;

    public UsersController(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok();
    }

    [HttpGet("{id:guid}", Name = "GetUserById")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery(id);
        var userResult = await _messageBus.InvokeAsync<Result<UserDto, Error>>(query, cancellationToken);

        if (userResult.IsFailure)
        {
            if (!string.IsNullOrEmpty(userResult.Error.Code) && userResult.Error.Code.Contains("notfound"))
                return NotFound(userResult.Error);

            return BadRequest(userResult.Error);
        }

        return Ok(userResult.Value);
    }
}
