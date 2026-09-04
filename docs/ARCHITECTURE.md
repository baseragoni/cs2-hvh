# Architecture

Two projects under `src/` — flat folders, more modules as separate `.cs` files.

```
src/
  App/
    Program.cs          # entry + settings load
    Commands.cs         # CLI command handlers
    CliUtils.cs         # args + table output
    appsettings.json
  Core/
    Models.cs           # vault, account, portfolio, fee, sync types
    Contracts.cs        # interfaces + JSON defaults
    Codecs.cs           # hex, base58, bech32-style
    VaultCrypto.cs      # AES-GCM + PBKDF2
    MnemonicService.cs  # BIP39-style lab mnemonics
    Derivation.cs       # HD child keys + address factory
    Networks.cs         # registry + endpoint rotator
    ChainClient.cs      # simulated RPC + fee estimator
    VaultStore.cs       # JSON persistence + schema migrations
    Validation.cs       # guards, tx builder, portfolio analytics
    Services.cs         # discovery, sync, export
    WalletService.cs    # facade / composition root
tests/
  Core.Tests/
```

All chain I/O is simulated (`ChainClient`). Vaults are JSON files under `.wallets/`.
