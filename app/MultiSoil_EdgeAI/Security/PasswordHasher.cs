using System.Security.Cryptography;
using System.Text;

namespace MultiSoil_EdgeAI.Security;

public static class PasswordHasher
{
    public static (string Hash, string Salt) HashPassword(string password, string? salt = null)
    {
        salt ??= Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password + salt);
        var hash = Convert.ToBase64String(sha.ComputeHash(bytes));
        return (hash, salt);
    }

    public static bool Verify(string password, string hash, string salt)
        => HashPassword(password, salt).Hash == hash;
}
