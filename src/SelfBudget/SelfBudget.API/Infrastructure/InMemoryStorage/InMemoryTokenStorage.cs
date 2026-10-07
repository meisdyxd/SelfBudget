using SelfBudget.API.Application.Abstractions;
using SelfBudget.API.Common.Dtos.AuthDtos;
using System.Collections.Concurrent;

namespace SelfBudget.API.Infrastructure.InMemoryStorage;

public class InMemoryTokenStorage : ITokenStorage
{
    private readonly ConcurrentDictionary<Guid, AccessAndRefreshToken?> Data = [];
    private readonly ILogger<InMemoryTokenStorage> _logger;

    public InMemoryTokenStorage(ILogger<InMemoryTokenStorage> logger)
    {
        _logger = logger;
    }

    public Task<AccessAndRefreshToken?> GetAccessAndRefreshTokenByUserId(Guid userId)
    {
        if (Data.TryGetValue(userId, out AccessAndRefreshToken? value))
            return Task.FromResult(value);

        return Task.FromResult<AccessAndRefreshToken?>(null);
    }

    public Task<string?> GetAccessTokenByUserId(Guid userId)
    {
        if (Data.TryGetValue(userId, out AccessAndRefreshToken? value))
            return Task.FromResult<string?>(value!.AccessToken);

        return Task.FromResult<string?>(null);
    }

    public Task<string?> GetRefreshTokenByUserId(Guid userId)
    {
        if (Data.TryGetValue(userId, out AccessAndRefreshToken? value))
            return Task.FromResult<string?>(value!.RefreshToken);

        return Task.FromResult<string?>(null);
    }

    public Task SetAccessAndRefreshTokenByUserId(Guid userId, AccessAndRefreshToken accessAndRefreshToken)
    {
        Data[userId] = accessAndRefreshToken;
        return Task.CompletedTask;
    }

    public Task SetAccessTokenByUserId(Guid userId, string accessToken)
    {
        if (Data.TryGetValue(userId, out AccessAndRefreshToken? value))
        {
            var tokens = value! with { AccessToken = accessToken };
            Data[userId] = tokens;

            return Task.CompletedTask;
        }
        else
        {
            _logger.LogError("Неизвестная сессия для пользователя с ID: {UserId}", userId);
            throw new ArgumentException($"Неизвестная сессия для пользователя с ID: {userId}");
        }
    }

    public Task SetRefreshTokenByUserId(Guid userId, string refreshToken)
    {
        if (Data.TryGetValue(userId, out AccessAndRefreshToken? value))
        {
            var tokens = value! with { RefreshToken = refreshToken };
            Data[userId] = tokens;

            return Task.CompletedTask;
        }
        else
        {
            _logger.LogError("Неизвестная сессия для пользователя с ID: {UserId}", userId);
            throw new ArgumentException($"Неизвестная сессия для пользователя с ID: {userId}");
        }
    }

    public Task Deactivate(Guid userId)
    {
        Data[userId] = null;
        return Task.CompletedTask;
    }
}
