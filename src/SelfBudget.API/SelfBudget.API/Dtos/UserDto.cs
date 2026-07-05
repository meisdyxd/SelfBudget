namespace SelfBudget.API.Dtos;

public sealed record UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public Guid? PhotoId { get; set; }
    public DateTime Birthdate { get; set; }
}
