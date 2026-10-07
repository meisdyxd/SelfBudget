using SelfBudget.API.Common.Dtos.AuthDtos;
using System.Security.Claims;

namespace SelfBudget.API.Application.Abstractions;

public interface ITokenProvider
{
    string RefreshAccessToken(AccessAndRefreshToken accessAndRefreshToken);
    AccessAndRefreshToken GenerateAccessAndRefreshToken(IEnumerable<Claim> claims);
    string ValidateAccessToken(Guid userId, string accessToken);
}
