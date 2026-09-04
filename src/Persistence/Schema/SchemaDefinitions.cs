namespace Cs2Hvh.Persistence.Schema;

internal static class SchemaDefinitions
{
    public static IReadOnlyList<string> AllMigrations { get; } = new[]
    {
        """
        CREATE TABLE IF NOT EXISTS vaults (
            vault_id TEXT PRIMARY KEY,
            label TEXT NOT NULL,
            encrypted_seed BLOB NOT NULL,
            salt BLOB NOT NULL,
            nonce BLOB NOT NULL,
            kdf_iterations INTEGER NOT NULL,
            created_at TEXT NOT NULL,
            modified_at TEXT NULL,
            enabled_networks TEXT NOT NULL
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS accounts (
            account_id TEXT PRIMARY KEY,
            vault_id TEXT NOT NULL,
            network_id TEXT NOT NULL,
            derivation_path TEXT NOT NULL,
            public_address TEXT NOT NULL,
            account_index INTEGER NOT NULL,
            address_index INTEGER NOT NULL,
            created_at TEXT NOT NULL,
            last_synced_at TEXT NULL,
            sync_status INTEGER NOT NULL,
            FOREIGN KEY (vault_id) REFERENCES vaults(vault_id)
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS sync_state (
            vault_id TEXT NOT NULL,
            network_id TEXT NOT NULL,
            last_block INTEGER NOT NULL,
            last_tx_hash TEXT NULL,
            started_at TEXT NOT NULL,
            completed_at TEXT NULL,
            phase INTEGER NOT NULL,
            accounts INTEGER NOT NULL,
            tx_count INTEGER NOT NULL,
            error TEXT NULL,
            PRIMARY KEY (vault_id, network_id)
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS transactions (
            tx_hash TEXT PRIMARY KEY,
            vault_id TEXT NOT NULL,
            network_id TEXT NOT NULL,
            from_addr TEXT NOT NULL,
            to_addr TEXT NOT NULL,
            value REAL NOT NULL,
            asset TEXT NOT NULL,
            block_number INTEGER NOT NULL,
            confirmations INTEGER NOT NULL,
            direction INTEGER NOT NULL,
            status INTEGER NOT NULL,
            fee REAL NULL,
            timestamp TEXT NOT NULL
        );
        """,
        "CREATE INDEX IF NOT EXISTS idx_accounts_vault ON accounts(vault_id);",
        "CREATE INDEX IF NOT EXISTS idx_tx_vault ON transactions(vault_id);"
    };
}
