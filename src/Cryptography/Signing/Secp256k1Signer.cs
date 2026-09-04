using System.Security.Cryptography;

namespace Cs2Hvh.Cryptography.Signing;

public sealed class Secp256k1Signer
{
    public byte[] Sign(ReadOnlySpan<byte> messageHash, ReadOnlySpan<byte> privateKey)
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        return ecdsa.SignHash(messageHash.ToArray());
    }

    public bool Verify(ReadOnlySpan<byte> messageHash, ReadOnlySpan<byte> signature, ReadOnlySpan<byte> publicKey)
    {
        using var ecdsa = ECDsa.Create();
        ecdsa.ImportSubjectPublicKeyInfo(publicKey.ToArray(), out _);
        return ecdsa.VerifyHash(messageHash.ToArray(), signature.ToArray());
    }
}
