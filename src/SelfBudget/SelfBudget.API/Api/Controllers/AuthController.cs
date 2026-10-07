using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using SelfBudget.API.Application.UseCases.AuthUseCases.Login;
using SelfBudget.API.Application.UseCases.AuthUseCases.Register;
using SelfBudget.API.Common;
using SelfBudget.API.Common.Dtos.Requests.AuthRequests;
using SelfBudget.API.Common.Dtos.Responses.AuthResponses;
using Wolverine;

namespace SelfBudget.API.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> CreateUser(
        [FromBody] RegisterRequest request,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        var command = RegisterCommand.FromRequest(request);

        var result = await messageBus.InvokeAsync<Result<RegisterResponse, Error>>(command, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error.Code is not null && result.Error.Code.Contains("conflict"))
                return Conflict(result.Error);

            return BadRequest(result.Error);
        }

        var response = result.Value;

        return CreatedAtRoute("GetUserById", new { id = response.Id}, response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        var command = LoginCommand.FromRequest(request);

        var result = await messageBus.InvokeAsync<Result<LoginResponse, Error>>(command, cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        var response = result.Value;

        return Ok(response);
    }
}
