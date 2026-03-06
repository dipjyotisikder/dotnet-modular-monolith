using System.Security.Cryptography;

namespace Users.Infrastructure.Options;

public class PasswordHasherOptions
{
    public int SaltSize { get; set; } = 16;
    public int HashSize { get; set; } = 32;
    public int Iterations { get; set; } = 100000;
    public HashAlgorithmName Algorithm { get; set; } = HashAlgorithmName.SHA256;
}
