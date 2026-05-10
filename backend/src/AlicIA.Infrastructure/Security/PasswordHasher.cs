using System.Security.Cryptography;
using System.Text;

namespace AlicIA.Infrastructure.Security;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 20;
    private const int Iterations = 10000;

    public string HashPassword(string password)
    {
        using var algorithm = new Rfc2898DeriveBytes(
            password,
            SaltSize,
            Iterations,
            HashAlgorithmName.SHA256);

        var key = Convert.ToBase64String(algorithm.GetBytes(HashSize));
        var salt = Convert.ToBase64String(algorithm.Salt);

        return $"$PBKDF2$SHA256${Iterations}${salt}${key}";
    }

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            var parts = hash.Split('$');
            if (parts.Length != 6 || parts[1] != "PBKDF2")
                return false;

            var iterations = int.Parse(parts[3]);
            var salt = Convert.FromBase64String(parts[4]);
            var key = Convert.FromBase64String(parts[5]);

            using var algorithm = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256);

            var keyToCheck = algorithm.GetBytes(HashSize);
            return CryptographicOperations.FixedTimeEquals(key, keyToCheck);
        }
        catch
        {
            return false;
        }
    }
}
