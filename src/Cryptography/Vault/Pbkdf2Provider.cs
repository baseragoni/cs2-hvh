using System.Security.Cryptography;

namespace Cs2Hvh.Cryptography.Vault;

public static class Pbkdf2Provider
{
    public static byte[] Derive(byte[] password, byte[] salt, int iterations, int length)
    {
        return Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, length);
    }
}
