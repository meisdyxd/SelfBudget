using CSharpFunctionalExtensions;
using SelfBudget.API.Application.Abstractions.Repositories;
using SelfBudget.API.Common;
using SelfBudget.API.Common.Dtos.Responses;

namespace SelfBudget.API.Application.UseCases.TransferUseCases.GetTransfer;

public class GetTransferHandler
{
    private readonly ITransactionRepository _transactionRepository;

    public GetTransferHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<Result<TransferResponse, Error>> Handle(
        GetTransferQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _transactionRepository.GetTransactionByIdAsync(query.Id, cancellationToken);
        return result;
    }
}
