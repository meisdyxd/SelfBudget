namespace SelfBudget.API.Common.Dtos.UserDtos;

public sealed record UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid? PhotoId { get; set; }
    public DateTime Birthdate { get; set; }
}
