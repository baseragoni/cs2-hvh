using Cs2Hvh.Core;
using Xunit;

namespace Cs2Hvh.Core.Tests;

public class CryptoTests
{
    [Fact]
    public void Vault_roundtrips_seed()
    {
        var crypto = new VaultCrypto();
        var (cipher, salt) = crypto.Encrypt("hello-seed", "pass");
        Assert.Equal("hello-seed", crypto.Decrypt(cipher, salt, "pass"));
    }

    [Fact]
    public void Fingerprint_is_stable()
    {
        var crypto = new VaultCrypto();
        var seed = new byte[32];
        seed[0] = 7;
        Assert.Equal(crypto.Fingerprint(seed), crypto.Fingerprint(seed));
    }

    [Fact]
    public void Mnemonic_derives_stable_address()
    {
        var m = new MnemonicService();
        var mnemonic = m.GenerateLabMnemonic();
        Assert.True(m.Validate(mnemonic));
        Assert.Equal(128, m.EntropyBits(mnemonic));
        var seed = m.DeriveSeed(mnemonic, "lab");
        var factory = new AddressFactory();
        var net = new NetworkRegistry().Get("ethereum-mainnet");
        var a1 = factory.Derive(seed, net, 0);
        var a2 = factory.Derive(seed, net, 0);
        Assert.Equal(a1, a2);
        Assert.StartsWith("0x", a1);
    }

    [Fact]
    public void Base58_encodes_and_decodes()
    {
        var bytes = new byte[] { 1, 2, 3, 4, 5 };
        var encoded = Base58Encoder.Encode(bytes);
        Assert.False(string.IsNullOrWhiteSpace(encoded));
        var decoded = Base58Encoder.Decode(encoded);
        Assert.NotEmpty(decoded);
        Assert.Equal(bytes[^1], decoded[^1]);
    }

    [Fact]
    public void Hex_roundtrip()
    {
        var bytes = new byte[] { 0xde, 0xad, 0xbe, 0xef };
        Assert.Equal(bytes, HexCodec.FromHex(HexCodec.ToHex(bytes)));
        Assert.True(HexCodec.TryFromHex("0xdead", out var b));
        Assert.Equal(new byte[] { 0xde, 0xad }, b);
    }
}

public class NetworkTests
{
    [Fact]
    public void Registry_has_core_networks()
    {
        var reg = new NetworkRegistry();
        Assert.True(reg.Exists("bitcoin-mainnet"));
        Assert.True(reg.Exists("ethereum-mainnet"));
        Assert.Equal("BTC", reg.Get("bitcoin-mainnet").Symbol);
        Assert.Contains(reg.ByKind("EVM"), n => n.Id == "polygon-mainnet");
    }

    [Fact]
    public void EndpointRotator_cycles()
    {
        var net = new NetworkRegistry().Get("ethereum-mainnet");
        var rotator = new EndpointRotator();
        var a = rotator.Next(net);
        var b = rotator.Next(net);
        Assert.False(string.IsNullOrWhiteSpace(a));
        Assert.False(string.IsNullOrWhiteSpace(b));
    }

    [Fact]
    public async Task ChainClient_returns_deterministic_balance()
    {
        var client = new ChainClient(new NetworkRegistry());
        var a = await client.FetchBalanceAsync("ethereum-mainnet", "0xabc");
        var b = await client.FetchBalanceAsync("ethereum-mainnet", "0xabc");
        Assert.Equal(a, b);
        var txs = await client.FetchRecentAsync("ethereum-mainnet", "0xabc", 3);
        Assert.Equal(3, txs.Count);
    }

    [Fact]
    public void FeeEstimator_policies_differ()
    {
        var fees = new FeeEstimator();
        var economy = fees.Quote("bitcoin-mainnet", "economy");
        var fast = fees.Quote("bitcoin-mainnet", "fast");
        Assert.True(fast.SuggestedFee > economy.SuggestedFee);
    }
}

public class WalletTests
{
    [Fact]
    public async Task Import_sync_and_status()
    {
        var dir = Path.Combine(Path.GetTempPath(), "cs2hvh-test-" + Guid.NewGuid().ToString("N"));
        try
        {
            var svc = WalletService.Create(new WalletOptions { DefaultVaultDirectory = dir, MaxAccountsPerNetwork = 2 });
            var vault = await svc.ImportAsync("t", svc.Mnemonic.GenerateLabMnemonic(), "lab");
            Assert.NotEmpty(vault.Accounts);
            Assert.False(string.IsNullOrEmpty(vault.Fingerprint));

            var report = await svc.SyncAsync(vault.Id);
            Assert.Equal(vault.Id, report.VaultId);
            Assert.True(report.AccountsTouched >= 1);

            var status = await svc.StatusAsync();
            Assert.Equal(1, status.VaultCount);
            Assert.True(status.AccountCount >= 1);

            var quote = svc.QuoteFee("bitcoin-mainnet", "standard");
            Assert.True(quote.SuggestedFee > 0);
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Validation_rejects_empty_label()
    {
        Assert.Throws<ArgumentException>(() => Validation.EnsureLabel(""));
    }

    [Fact]
    public void Portfolio_allocation_sums()
    {
        var analytics = new PortfolioAnalytics();
        var summary = analytics.Summarize(
        [
            new WalletVault
            {
                Id = "a",
                Label = "a",
                EncryptedSeed = "x",
                Salt = "y",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Accounts =
                [
                    new WalletAccount { NetworkId = "ethereum-mainnet", Address = "0x1", Symbol = "ETH", Balance = 2 },
                    new WalletAccount { NetworkId = "bitcoin-mainnet", Address = "bc1q", Symbol = "BTC", Balance = 2 }
                ]
            }
        ]);
        var alloc = analytics.AllocationPercents(summary);
        Assert.Equal(50m, alloc["ETH"]);
        Assert.Equal(50m, alloc["BTC"]);
    }
}
