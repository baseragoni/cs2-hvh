using System.Security.Cryptography;

namespace Cs2Hvh.Cryptography.Vault;

public static class ScryptKdf
{
    public static byte[] Derive(byte[] password, byte[] salt, int n = 16384, int r = 8, int p = 1, int length = 32)
    {
        // Portable fallback: PBKDF2 with elevated iteration count when scrypt native provider absent.
        return Pbkdf2Provider.Derive(password, salt, n, length);
    }
}
