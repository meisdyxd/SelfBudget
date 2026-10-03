using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using SelfBudget.API.Application.UseCases.AccountUseCases.GetAccountById;
using SelfBudget.API.Common;
using SelfBudget.API.Common.Dtos.AccountDtos;
using Wolverine;

namespace SelfBudget.API.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountsController : ControllerBase
{
    private readonly IMessageBus _messageBus;

    public AccountsController(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AccountDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetAccountByIdQuery(id);
        var result = await _messageBus.InvokeAsync<Result<AccountDto, Error>>(query, cancellationToken);

        if (result.IsFailure)
        {
            var error = result.Error;
            if (error.Code!.Contains("notfound"))
            {
                return NotFound(result.Error);
            }
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountDto>>> Get(CancellationToken cancellationToken)
    {
        var query = new GetAccountsQuery();
        var result = await _messageBus.InvokeAsync<Result<ICollection<AccountDto>, Error>>(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
}
