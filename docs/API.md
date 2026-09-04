# API Reference

## WalletManager

| Method | Description |
|--------|-------------|
| `InitializeAsync()` | Opens SQLite store and applies schema migrations |
| `ImportMnemonicAsync(label, mnemonic, passphrase, networks)` | Creates encrypted vault and derives initial accounts |
| `ListVaultsAsync()` | Returns all vault metadata entries |
| `GetAccountsAsync(vaultId, networkId?)` | Lists derived accounts with cached balances |

## SyncCoordinator

| Method | Description |
|--------|-------------|
| `RunFullSyncAsync(vaultId, networkId)` | Executes full discover → fetch → index → persist cycle |

## CLI Commands

```bash
cs2hvh import --label "main" --mnemonic "..." --passphrase "..."
cs2hvh list
cs2hvh sync <vault-id>
cs2hvh balance <vault-id>
cs2hvh export <vault-id>
```

## Configuration (`appsettings.json`)

| Key | Default | Description |
|-----|---------|-------------|
| `Wallet:DefaultVaultDirectory` | `.wallets` | Local vault storage path |
| `Wallet:DefaultAccountScanDepth` | `5` | Accounts derived per network on import |
| `Wallet:MaxConcurrentNetworkRequests` | `4` | RPC concurrency limit |
| `Wallet:RpcTimeoutSeconds` | `30` | HTTP timeout for node requests |
| `Wallet:EnabledNetworks` | `[eth, btc, polygon]` | Default networks for new vaults |
