using System.Security.Cryptography;
using System.Text;

namespace Cs2Hvh.Core;

public sealed class HdDerivation
{
    public byte[] ChildKey(byte[] seed, int purpose, int coinType, int account, int change, int index)
    {
        var path = $"{purpose}/{coinType}/{account}/{change}/{index}";
        var material = Encoding.UTF8.GetBytes(path);
        var hmac = HMACSHA512.HashData(seed, material);
        return hmac.AsSpan(0, 32).ToArray();
    }

    public string FormatPath(NetworkDescriptor network, int accountIndex, int change = 0) =>
        $"{network.DerivationPath}/{accountIndex}" + (change > 0 ? $"/{change}" : "");

    public byte[] MasterFingerprint(byte[] seed) => SHA256.HashData(seed).AsSpan(0, 4).ToArray();
}

public sealed class AddressFactory : IAddressFactory
{
    private readonly HdDerivation _hd = new();

    public string Derive(byte[] seed, NetworkDescriptor network, int accountIndex, int change = 0)
    {
        var child = _hd.ChildKey(seed, 44, network.CoinType, 0, change, accountIndex);
        var hash = SHA256.HashData(child);

        if (network.Kind.Equals("UTXO", StringComparison.OrdinalIgnoreCase))
        {
            var hrp = network.IsTestnet ? "tb" : "bc";
            return Bech32Style.Encode(hrp + "1q", hash.AsSpan(0, 20));
        }

        return HexCodec.ToHexPrefixed(hash.AsSpan(0, 20));
    }

    public bool LooksValid(string address, NetworkDescriptor network)
    {
        if (string.IsNullOrWhiteSpace(address)) return false;
        if (network.Kind.Equals("UTXO", StringComparison.OrdinalIgnoreCase))
            return address.StartsWith("bc1", StringComparison.OrdinalIgnoreCase)
                   || address.StartsWith("tb1", StringComparison.OrdinalIgnoreCase);
        return address.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && address.Length == 42;
    }
}
