using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SelfBudget.API.Application.Options;

public class AuthOptions
{
    public static string SectionName = "AuthOptions";
    [Required]
    public string Issuer { get; set; } = null!;
    [Required]
    public string Audience { get; set; } = null!;
    [Required]
    public string SecretKey { get; set; } = null!;
    public int AccessTokenLifespanSeconds { get; set; } = 900;
    public int RefreshTokenLifespanSeconds { get; set; } = 3600;

    public SymmetricSecurityKey GetSymmetricSecurityKey() =>
        new(Encoding.UTF8.GetBytes(SecretKey));
}
