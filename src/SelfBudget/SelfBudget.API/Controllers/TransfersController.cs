using Microsoft.AspNetCore.Mvc;
using SelfBudget.API.Dtos.Requests;
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
        var result = await _messageBus.InvokeAsync<TransferResponse>(command, cancellationToken);
        return CreatedAtAction(nameof(GetTransfer), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TransferResponse>> GetTransfer(Guid id, CancellationToken cancellationToken)
    {
        var query = GetTransferQuery.FromRequest(id);
        var result = await _messageBus.InvokeAsync<TransferResponse>(query, cancellationToken);
        return Ok(result);
    }
}
