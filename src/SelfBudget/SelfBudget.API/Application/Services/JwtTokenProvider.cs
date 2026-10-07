using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SelfBudget.API.Application.Abstractions;
using SelfBudget.API.Application.Options;
using SelfBudget.API.Common.Dtos.AuthDtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SelfBudget.API.Application.Services;

public class JwtTokenProvider : ITokenProvider
{
    private readonly AuthOptions _options;
    private DateTime GetExpiresForRefresh => DateTime.UtcNow + TimeSpan.FromSeconds(_options.RefreshTokenLifespanSeconds);
    private DateTime GetExpiresForAccess => DateTime.UtcNow + TimeSpan.FromSeconds(_options.AccessTokenLifespanSeconds);

    public JwtTokenProvider(IOptionsMonitor<AuthOptions> options)
    {
        _options = options.CurrentValue;
    }

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
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(_options.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
            );
        var handler = new JwtSecurityTokenHandler();
        
        return (handler.WriteToken(jwt), expiresAt);
    }

    private (string Token, DateTime ExpiresAt) GenerateRefreshToken(IEnumerable<Claim> claims)
    {
        var expiresAt = GetExpiresForRefresh;
        var jwt = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(_options.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
            );
        var handler = new JwtSecurityTokenHandler();

        return (handler.WriteToken(jwt), expiresAt);
    }
}
