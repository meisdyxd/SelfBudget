using CSharpFunctionalExtensions;
using SelfBudget.API.Application.Abstractions.Repositories;
using SelfBudget.API.Application.UseCases.UserUseCases.GetUserById;
using SelfBudget.API.Common;
using SelfBudget.API.Common.Dtos.AccountDtos;
using SelfBudget.API.Common.Dtos.UserDtos;

namespace SelfBudget.API.Application.UseCases.AccountUseCases.GetAccountById;

public class GetUserByIdHandler
{
    private readonly IUserRepository _repository;

    public GetUserByIdHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<UserDto, Error>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(query.Id, cancellationToken);

        if (user is null)
            return new Error($"Не найден пользователь с ID: {query.Id}", "error.user.notfound");

        return new UserDto
        {
            Id = user.Id,
            Birthdate = user.Birthdate,
            Email = user.Email,
            Name = user.Name,
            PhotoId = user.PhotoId
        };
    }
}
