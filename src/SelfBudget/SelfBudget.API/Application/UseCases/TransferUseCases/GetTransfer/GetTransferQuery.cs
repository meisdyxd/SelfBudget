namespace SelfBudget.API.Application.UseCases.TransferUseCases.GetTransfer;

public class GetTransferQuery
{
    public Guid Id { get; set; }

    public static GetTransferQuery FromRequest(Guid id)
    {
        return new()
        {
            Id = id
        };
    }
}
