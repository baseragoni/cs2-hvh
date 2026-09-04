using Cs2Hvh.Engine.Domain.Extensions;

namespace Cs2Hvh.Engine.Networks;

/// <summary>
/// Validates address formats across EVM and UTXO chain families.
/// </summary>
public sealed class AddressValidator
{
    public bool IsValidEvmAddress(string address) =>
        address.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
        && address.Length == 42
        && address.LooksLikeHexAddress();

    public bool IsValidBase58CheckAddress(string address, int minLength = 26, int maxLength = 62)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return false;
        }

        return address.Length >= minLength && address.Length <= maxLength
            && address.All(static c => char.IsLetterOrDigit(c));
    }

    public string NormalizeEvmAddress(string address)
    {
        if (!IsValidEvmAddress(address))
        {
            throw new ArgumentException("Invalid EVM address.", nameof(address));
        }

        return address.ToLowerInvariant();
    }
}
