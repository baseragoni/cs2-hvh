namespace Cs2Hvh.Core;

public sealed class Validation
{
    public static void EnsureLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Label is required.");
        if (label.Length > 64)
            throw new ArgumentException("Label too long (max 64).");
    }

    public static void EnsurePassphrase(string passphrase)
    {
        if (string.IsNullOrEmpty(passphrase))
            throw new ArgumentException("Passphrase is required.");
        if (passphrase.Length < 3)
            throw new ArgumentException("Passphrase too short for lab builds (min 3).");
    }

    public static void EnsureAmount(decimal amount, decimal dust)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
        if (amount < dust)
            throw new ArgumentOutOfRangeException(nameof(amount), $"Amount below dust threshold ({dust}).");
    }

    public static IReadOnlyList<string> NormalizeNetworks(IEnumerable<string>? networks, WalletOptions options, NetworkRegistry registry)
    {
        var list = (networks ?? options.EnabledNetworks)
            .Select(n => n.Trim())
            .Where(n => n.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(registry.Exists)
            .ToList();

        if (list.Count == 0)
            list = options.EnabledNetworks.Where(registry.Exists).ToList();

        if (list.Count == 0)
            throw new InvalidOperationException("No valid networks configured.");

        return list;
    }
}

public sealed class TransactionBuilder
{
    private readonly IFeeEstimator _fees;
    private readonly IAddressFactory _addresses;
    private readonly WalletOptions _options;

    public TransactionBuilder(IFeeEstimator fees, IAddressFactory addresses, WalletOptions options)
    {
        _fees = fees;
        _addresses = addresses;
        _options = options;
    }

    public UnsignedTransaction Build(
        NetworkDescriptor network,
        WalletAccount from,
        string to,
        decimal amount,
        string? feePolicy = null)
    {
        if (!_addresses.LooksValid(to, network) && !to.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && !to.StartsWith("bc1", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Destination address looks invalid.");

        Validation.EnsureAmount(amount, _options.DustThreshold);
        var quote = _fees.Quote(network.Id, feePolicy ?? _options.PreferredFeePolicy);
        var payload = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes($"{from.Address}>{to}:{amount}:{quote.SuggestedFee}"));

        return new UnsignedTransaction
        {
            NetworkId = network.Id,
            From = from.Address,
            To = to,
            Amount = amount,
            Fee = quote.SuggestedFee,
            PayloadHex = HexCodec.ToHex(payload)
        };
    }
}

public sealed class PortfolioAnalytics
{
    public PortfolioSummary Summarize(IEnumerable<WalletVault> vaults)
    {
        var list = vaults.ToList();
        var accounts = list.SelectMany(v => v.Accounts).ToList();
        var bySymbol = accounts
            .GroupBy(a => a.Symbol)
            .ToDictionary(g => g.Key, g => g.Sum(a => a.Balance), StringComparer.OrdinalIgnoreCase);

        return new PortfolioSummary
        {
            VaultCount = list.Count,
            AccountCount = accounts.Count,
            TotalUnits = accounts.Sum(a => a.Balance),
            TotalPending = accounts.Sum(a => a.Pending),
            Networks = accounts.Select(a => a.NetworkId).Distinct().OrderBy(x => x).ToList(),
            BySymbol = bySymbol,
            GeneratedAt = DateTimeOffset.UtcNow
        };
    }

    public IReadOnlyDictionary<string, decimal> AllocationPercents(PortfolioSummary summary)
    {
        if (summary.TotalUnits <= 0)
            return summary.BySymbol.ToDictionary(kv => kv.Key, _ => 0m);

        return summary.BySymbol.ToDictionary(
            kv => kv.Key,
            kv => Math.Round(kv.Value / summary.TotalUnits * 100m, 2));
    }
}
