using SelfBudget.API.Common.Dtos.AuthDtos;

namespace SelfBudget.API.Application.Abstractions;

public interface ITokenStorage
{
    Task<AccessAndRefreshToken?> GetAccessAndRefreshTokenByUserId(Guid userId);
    Task<string?> GetAccessTokenByUserId(Guid userId);
    Task<string?> GetRefreshTokenByUserId(Guid userId);
    Task SetAccessTokenByUserId(Guid userId, string accessToken);
    Task SetRefreshTokenByUserId(Guid userId, string refreshToken);
    Task SetAccessAndRefreshTokenByUserId(Guid userId, AccessAndRefreshToken accessAndRefreshToken);
}
