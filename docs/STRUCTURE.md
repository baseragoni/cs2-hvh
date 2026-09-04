# Repository structure

Every folder name answers one question: **what does this code do?**

```
src/
├── App/              What runs?          → cs2hvh console
├── Engine/           What decides?       → wallet logic, sync, validation
├── Cryptography/     What secures?       → keys, seeds, encryption
├── ChainProviders/   What talks to chain?→ RPC clients, transport
└── Persistence/      What remembers?     → SQLite vault database
```

## App

Console host. Nothing business-critical lives here — only command parsing, DI wiring, terminal output.

| Subfolder | Contents |
|-----------|----------|
| Commands/ | One file per CLI verb: import, sync, balance... |
| Bootstrap/ | ServiceBootstrapper, CommandRouter |
| Output/ | ConsoleOutputFormatter (colors, headers) |
| Program.cs | Entry point |
| appsettings.json | Default configuration |

## Engine

Core wallet brain. Split by responsibility, not by technical layer name.

| Subfolder | Contents |
|-----------|----------|
| Domain/Models/ | WalletVault, WalletAccount, TransactionRecord, SyncState |
| Domain/Contracts/ | IWalletStore, INetworkClient, IWalletProvider, IKeyDerivationService |
| Domain/Extensions/ | String and byte helpers |
| Orchestration/ | WalletManager, SyncCoordinator, DefaultWalletProvider, discovery |
| Analytics/ | BalanceAggregator, TransactionPipeline, PortfolioReporter |
| Networks/ | NetworkRegistry, AddressValidator, DerivationPathResolver |
| Validation/ | SeedPhraseValidator, TransactionValidator |
| Options/ | WalletOptions (appsettings binding) |

## Cryptography

Pure crypto — no wallet or network references.

| Subfolder | Contents |
|-----------|----------|
| Mnemonic/ | Word list provider, entropy collector |
| Derivation/ | Bip39MnemonicProcessor, Bip32KeyDeriver |
| Vault/ | VaultEncryptor, Pbkdf2Provider, ScryptKdf |
| Codecs/ | Base58, Bech32, Hex, SHA256, HMAC |
| Signing/ | Secp256k1Signer |

## ChainProviders

All blockchain I/O. Organized by chain family, not by class name prefix.

| Subfolder | Contents |
|-----------|----------|
| Evm/ | EthereumRpcClient, PolygonRpcClient, BscRpcClient |
| Utxo/ | BitcoinRpcClient |
| Transport/ | HttpTransportLayer, RpcClientBase, EndpointRotator, ConnectionPoolManager |
| Resilience/ | RpcRetryPolicy, CircuitBreakerState |

## Persistence

SQLite layer. Schema separate from stores.

| Subfolder | Contents |
|-----------|----------|
| Stores/ | SqliteWalletStore, MigrationRunner, caches |
| Schema/ | SchemaDefinitions (DDL migrations) |
| Repositories/ | VaultMetadataRepository, BalanceSnapshotRepository |
| Indexing/ | TransactionIndexBuilder |
| Maintenance/ | DatabaseVacuumService |

## tests/

| Project | Tests |
|---------|-------|
| Engine.Tests | NetworkRegistry, TransactionPipeline, DerivationPathResolver |
| Cryptography.Tests | Base58, VaultEncryptor, Bip39MnemonicProcessor |

## build/

All compiler output. Never commit. Configured via `UseArtifactsOutput` in Directory.Build.props.
