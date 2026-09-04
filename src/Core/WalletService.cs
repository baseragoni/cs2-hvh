namespace Cs2Hvh.Core;

public sealed class WalletService
{
    private readonly VaultStore _store;
    private readonly VaultCrypto _crypto;
    private readonly MnemonicService _mnemonic;
    private readonly NetworkRegistry _networks;
    private readonly ChainClient _chain;
    private readonly AddressFactory _addresses;
    private readonly AccountDiscovery _discovery;
    private readonly SyncCoordinator _sync;
    private readonly ExportService _export;
    private readonly PortfolioAnalytics _portfolio;
    private readonly TransactionBuilder _txBuilder;
    private readonly FeeEstimator _fees;
    private readonly WalletOptions _options;

    public WalletService(
        VaultStore store,
        VaultCrypto crypto,
        MnemonicService mnemonic,
        NetworkRegistry networks,
        ChainClient chain,
        AddressFactory addresses,
        WalletOptions options)
    {
        _store = store;
        _crypto = crypto;
        _mnemonic = mnemonic;
        _networks = networks;
        _chain = chain;
        _addresses = addresses;
        _options = options;
        _discovery = new AccountDiscovery(addresses, chain, options);
        _sync = new SyncCoordinator(chain, store);
        _export = new ExportService(store, chain);
        _portfolio = new PortfolioAnalytics();
        _fees = new FeeEstimator();
        _txBuilder = new TransactionBuilder(_fees, addresses, options);
    }

    public static WalletService Create(WalletOptions? options = null)
    {
        options ??= new WalletOptions();
        var networks = new NetworkRegistry();
        foreach (var (id, url) in options.EndpointOverrides)
        {
            if (!networks.Exists(id)) continue;
            var current = networks.Get(id);
            networks.Register(new NetworkDescriptor
            {
                Id = current.Id,
                Kind = current.Kind,
                Symbol = current.Symbol,
                CoinType = current.CoinType,
                Decimals = current.Decimals,
                DerivationPath = current.DerivationPath,
                SupportsReplaceByFee = current.SupportsReplaceByFee,
                IsTestnet = current.IsTestnet,
                Endpoints = new[] { url }.Concat(current.Endpoints).Distinct().ToList()
            });
        }

        var chain = new ChainClient(networks);
        var addresses = new AddressFactory();
        return new WalletService(
            new VaultStore(options.DefaultVaultDirectory),
            new VaultCrypto(),
            new MnemonicService(),
            networks,
            chain,
            addresses,
            options);
    }

    public async Task<WalletVault> ImportAsync(
        string label,
        string mnemonic,
        string passphrase,
        IEnumerable<string>? networkIds = null,
        CancellationToken ct = default)
    {
        Validation.EnsureLabel(label);
        Validation.EnsurePassphrase(passphrase);

        var normalized = _mnemonic.Normalize(mnemonic);
        if (!_mnemonic.Validate(normalized))
            throw new ArgumentException("Invalid mnemonic.");

        var seed = _mnemonic.DeriveSeed(normalized, passphrase);
        var (cipher, salt) = _crypto.Encrypt(Convert.ToBase64String(seed), passphrase);
        var nets = Validation.NormalizeNetworks(networkIds, _options, _networks);

        var vault = new WalletVault
        {
            Id = Guid.NewGuid().ToString("N")[..12],
            Label = label,
            EncryptedSeed = cipher,
            Salt = salt,
            Fingerprint = _crypto.Fingerprint(seed),
            SchemaVersion = MigrationRunner.CurrentSchema,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Networks = nets.ToList(),
            Metadata =
            {
                ["entropyBits"] = _mnemonic.EntropyBits(normalized).ToString(),
                ["importer"] = "cs2hvh"
            }
        };

        foreach (var netId in nets)
        {
            var network = _networks.Get(netId);
            var accounts = await _discovery.DiscoverAsync(seed, network, ct);
            vault.Accounts.AddRange(accounts);
        }

        await _store.SaveAsync(vault, ct);
        return vault;
    }

    public Task<IReadOnlyList<WalletVault>> ListAsync(CancellationToken ct = default) => _store.ListAsync(ct);

    public async Task<SyncReport> SyncAsync(string vaultId, CancellationToken ct = default)
    {
        var vault = await RequireVaultAsync(vaultId, ct);
        return await _sync.SyncVaultAsync(vault, ct);
    }

    public async Task<IReadOnlyList<WalletAccount>> BalancesAsync(string? vaultId = null, CancellationToken ct = default)
    {
        var vaults = vaultId is null
            ? await _store.ListAsync(ct)
            : [await RequireVaultAsync(vaultId, ct)];
        return vaults.SelectMany(v => v.Accounts).ToList();
    }

    public Task ExportAsync(string vaultId, string outputPath, CancellationToken ct = default) =>
        _export.ExportTransactionsAsync(vaultId, outputPath, ct: ct);

    public async Task<PortfolioSummary> StatusAsync(CancellationToken ct = default)
    {
        var vaults = await _store.ListAsync(ct);
        return _portfolio.Summarize(vaults);
    }

    public async Task<UnsignedTransaction> BuildTransferAsync(
        string vaultId,
        string networkId,
        string to,
        decimal amount,
        string? feePolicy = null,
        CancellationToken ct = default)
    {
        var vault = await RequireVaultAsync(vaultId, ct);
        var account = vault.Accounts.FirstOrDefault(a => a.NetworkId.Equals(networkId, StringComparison.OrdinalIgnoreCase))
                      ?? throw new InvalidOperationException($"No account for network '{networkId}'.");
        return _txBuilder.Build(_networks.Get(networkId), account, to, amount, feePolicy);
    }

    public FeeQuote QuoteFee(string networkId, string? policy = null) =>
        _fees.Quote(networkId, policy ?? _options.PreferredFeePolicy);

    public NetworkRegistry Networks => _networks;
    public MnemonicService Mnemonic => _mnemonic;
    public PortfolioAnalytics Portfolio => _portfolio;

    private async Task<WalletVault> RequireVaultAsync(string vaultId, CancellationToken ct) =>
        await _store.LoadAsync(vaultId, ct)
        ?? throw new InvalidOperationException($"Vault '{vaultId}' not found.");
}
