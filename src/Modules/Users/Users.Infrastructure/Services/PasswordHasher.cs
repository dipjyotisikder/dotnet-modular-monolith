using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Users.Domain.Services;
using Users.Infrastructure.Options;

namespace Users.Infrastructure.Services;

public sealed class PasswordHasher : IPasswordHasher
{
    private const string Version = "v1";
    private readonly PasswordHasherOptions _options;

    public PasswordHasher(IOptions<PasswordHasherOptions> options)
    {
        _options = options.Value;
    }

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(_options.SaltSize);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            _options.Iterations,
            _options.Algorithm,
            _options.HashSize);

        return string.Join('|',
            Version,
            _options.Iterations,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split('|');

        if (parts.Length != 4 || parts[0] != Version)
            return false;

        var iterations = int.Parse(parts[1]);
        var salt = Convert.FromBase64String(parts[2]);
        var expectedHash = Convert.FromBase64String(parts[3]);

        var computedHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            _options.Algorithm,
            expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(expectedHash, computedHash);
    }
}
