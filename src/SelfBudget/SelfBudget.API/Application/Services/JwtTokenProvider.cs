using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.IdentityModel.Tokens;
using SelfBudget.API.Application.Abstractions;
using SelfBudget.API.Common.Dtos.AuthDtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SelfBudget.API.Application.Services;

public class JwtTokenProvider : ITokenProvider
{
    private const string Issuer = "SelfBudget.Auth";
    private const string Audience = "SelfBudget.Backend";
    private const string KEY = "mysupersecret_secretsecretsecretkey!123";
    private static SymmetricSecurityKey GetSymmetricSecurityKey() =>
        new(Encoding.UTF8.GetBytes(KEY));
    private static DateTime GetExpiresForRefresh => DateTime.UtcNow + TimeSpan.FromHours(1);
    private static DateTime GetExpiresForAccess => DateTime.UtcNow + TimeSpan.FromMinutes(15);

    public AccessAndRefreshToken GenerateAccessAndRefreshToken(IEnumerable<Claim> claims)
    {
        var access = GenerateAccessToken(claims);
        var refresh = GenerateRefreshToken(claims);

        return new AccessAndRefreshToken(access.Token, refresh.Token, access.ExpiresAt, refresh.ExpiresAt);
    }

    public string RefreshAccessToken(AccessAndRefreshToken accessAndRefreshToken)
    {
        throw new NotImplementedException();
    }

    public string ValidateAccessToken(Guid userId, string accessToken)
    {
        throw new NotImplementedException();
    }

    private (string Token, DateTime ExpiresAt) GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var expiresAt = GetExpiresForAccess;
        var jwt = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
            );
        var handler = new JwtSecurityTokenHandler();
        
        return (handler.WriteToken(jwt), expiresAt);
    }

    private (string Token, DateTime ExpiresAt) GenerateRefreshToken(IEnumerable<Claim> claims)
    {
        var expiresAt = GetExpiresForRefresh;
        var jwt = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
            );
        var handler = new JwtSecurityTokenHandler();

        return (handler.WriteToken(jwt), expiresAt);
    }
}
