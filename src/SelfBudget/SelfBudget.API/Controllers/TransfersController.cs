using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using SelfBudget.API.Common;
using SelfBudget.API.Dtos.Requests.TransferRequests;
using SelfBudget.API.Dtos.Responses;
using SelfBudget.API.UseCases.GetTransfer;
using SelfBudget.API.UseCases.TransferBetweenAccounts;
using Wolverine;

namespace SelfBudget.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TransfersController : ControllerBase
{
    private readonly IMessageBus _messageBus;

    public TransfersController(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    [HttpPost]
    public async Task<ActionResult<TransferResponse>> CreateTransfer(
        [FromBody] TransferBetweenAccountsRequest request,
        CancellationToken cancellationToken)
    {
        var command = TransferBetweenAccountsCommand.FromRequest(request);
        var result = await _messageBus.InvokeAsync<Result<TransferResponse, Error>>(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetTransfer), new { id = result.Value.Id }, result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TransferResponse>> GetTransfer(Guid id, CancellationToken cancellationToken)
    {
        var query = GetTransferQuery.FromRequest(id);
        var result = await _messageBus.InvokeAsync<Result<TransferResponse, Error>>(query, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
