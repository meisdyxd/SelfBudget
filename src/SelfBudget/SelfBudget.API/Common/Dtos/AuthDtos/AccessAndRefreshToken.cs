namespace SelfBudget.API.Common.Dtos.AuthDtos;

public record AccessAndRefreshToken(string AccessToken, string RefreshToken, DateTime AccessExpiresAt, DateTime RefreshExpiresAt);
