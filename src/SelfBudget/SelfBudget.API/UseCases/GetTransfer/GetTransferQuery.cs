using SelfBudget.API.Dtos;

namespace SelfBudget.API.UseCases.GetTransfer;

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
