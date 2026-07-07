using System.Security.Cryptography;
using System.Text;

namespace SelfBudget.API.Application.Services;

public static class PasswordProvider
{
    public static bool ValidatePassword(string password, string hashPassword)
    {
        return HashPassword(password) == hashPassword;
    }

    public static string HashPassword(string password)
    {
        using var sha512 = SHA512.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha512.ComputeHash(bytes);

        var hashedPassword = new StringBuilder();
        foreach (var b in hash)
        {
            hashedPassword.Append(b.ToString("x2"));
        }

        return hashedPassword.ToString();
    }
}
