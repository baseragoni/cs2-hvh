using Microsoft.Data.Sqlite;
using Cs2Hvh.Engine.Domain.Contracts;
using Cs2Hvh.Engine.Domain.Models;

namespace Cs2Hvh.Persistence.Stores;

public sealed class SqliteWalletStore : IWalletStore
{
    private readonly string _connectionString;

    public SqliteWalletStore(string databasePath)
    {
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await new MigrationRunner(connection).ApplyAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task SaveVaultAsync(WalletVault vault, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = """
            INSERT OR REPLACE INTO vaults
            (vault_id, label, encrypted_seed, salt, nonce, kdf_iterations, created_at, modified_at, enabled_networks)
            VALUES ($id, $label, $seed, $salt, $nonce, $kdf, $created, $modified, $networks)
            """;

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$id", vault.VaultId);
        command.Parameters.AddWithValue("$label", vault.Label);
        command.Parameters.AddWithValue("$seed", vault.EncryptedSeedBlob);
        command.Parameters.AddWithValue("$salt", vault.Salt);
        command.Parameters.AddWithValue("$nonce", vault.Nonce);
        command.Parameters.AddWithValue("$kdf", vault.KdfIterations);
        command.Parameters.AddWithValue("$created", vault.CreatedAt.UtcDateTime);
        command.Parameters.AddWithValue("$modified", (object?)vault.ModifiedAt?.UtcDateTime ?? DBNull.Value);
        command.Parameters.AddWithValue("$networks", string.Join(',', vault.EnabledNetworkIds));

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<WalletVault?> GetVaultAsync(string vaultId, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = "SELECT * FROM vaults WHERE vault_id = $id LIMIT 1";
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$id", vaultId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return new WalletVault
        {
            VaultId = reader.GetString(0),
            Label = reader.GetString(1),
            EncryptedSeedBlob = (byte[])reader["encrypted_seed"],
            Salt = (byte[])reader["salt"],
            Nonce = (byte[])reader["nonce"],
            KdfIterations = reader.GetInt32(5),
            CreatedAt = reader.GetDateTime(6),
            ModifiedAt = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
            EnabledNetworkIds = reader.GetString(8).Split(',', StringSplitOptions.RemoveEmptyEntries)
        };
    }

    public async Task<IReadOnlyList<WalletVault>> ListVaultsAsync(CancellationToken cancellationToken)
    {
        var vaults = new List<WalletVault>();
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT vault_id FROM vaults ORDER BY created_at DESC";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var vault = await GetVaultAsync(reader.GetString(0), cancellationToken).ConfigureAwait(false);
            if (vault is not null)
            {
                vaults.Add(vault);
            }
        }

        return vaults;
    }

    public async Task SaveAccountsAsync(string vaultId, IEnumerable<WalletAccount> accounts, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        foreach (var account in accounts)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = (SqliteTransaction)transaction;
            command.CommandText = """
                INSERT OR REPLACE INTO accounts
                (account_id, vault_id, network_id, derivation_path, public_address, account_index, address_index, created_at, last_synced_at, sync_status)
                VALUES ($id, $vault, $network, $path, $address, $accIdx, $addrIdx, $created, $synced, $status)
                """;
            command.Parameters.AddWithValue("$id", account.AccountId);
            command.Parameters.AddWithValue("$vault", vaultId);
            command.Parameters.AddWithValue("$network", account.NetworkId);
            command.Parameters.AddWithValue("$path", account.DerivationPath);
            command.Parameters.AddWithValue("$address", account.PublicAddress);
            command.Parameters.AddWithValue("$accIdx", account.AccountIndex);
            command.Parameters.AddWithValue("$addrIdx", account.AddressIndex);
            command.Parameters.AddWithValue("$created", account.CreatedAt.UtcDateTime);
            command.Parameters.AddWithValue("$synced", (object?)account.LastSyncedAt?.UtcDateTime ?? DBNull.Value);
            command.Parameters.AddWithValue("$status", (int)account.SyncStatus);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<WalletAccount>> GetAccountsAsync(string vaultId, string? networkId, CancellationToken cancellationToken)
    {
        var accounts = new List<WalletAccount>();
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = networkId is null
            ? "SELECT * FROM accounts WHERE vault_id = $vault"
            : "SELECT * FROM accounts WHERE vault_id = $vault AND network_id = $network";
        command.Parameters.AddWithValue("$vault", vaultId);
        if (networkId is not null)
        {
            command.Parameters.AddWithValue("$network", networkId);
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            accounts.Add(new WalletAccount
            {
                AccountId = reader.GetString(0),
                NetworkId = reader.GetString(2),
                DerivationPath = reader.GetString(3),
                PublicAddress = reader.GetString(4),
                AccountIndex = reader.GetInt32(5),
                AddressIndex = reader.GetInt32(6),
                CreatedAt = reader.GetDateTime(7),
                LastSyncedAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                SyncStatus = (AccountSyncStatus)reader.GetInt32(9)
            });
        }

        return accounts;
    }

    public async Task SaveSyncStateAsync(SyncState state, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT OR REPLACE INTO sync_state
            (vault_id, network_id, last_block, last_tx_hash, started_at, completed_at, phase, accounts, tx_count, error)
            VALUES ($vault, $network, $block, $tx, $started, $completed, $phase, $accounts, $txCount, $error)
            """;
        command.Parameters.AddWithValue("$vault", state.VaultId);
        command.Parameters.AddWithValue("$network", state.NetworkId);
        command.Parameters.AddWithValue("$block", state.LastProcessedBlock);
        command.Parameters.AddWithValue("$tx", (object?)state.LastProcessedTransactionHash ?? DBNull.Value);
        command.Parameters.AddWithValue("$started", state.LastSyncStartedAt.UtcDateTime);
        command.Parameters.AddWithValue("$completed", (object?)state.LastSyncCompletedAt?.UtcDateTime ?? DBNull.Value);
        command.Parameters.AddWithValue("$phase", (int)state.CurrentPhase);
        command.Parameters.AddWithValue("$accounts", state.AccountsDiscovered);
        command.Parameters.AddWithValue("$txCount", state.TransactionsIndexed);
        command.Parameters.AddWithValue("$error", (object?)state.LastErrorMessage ?? DBNull.Value);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<SyncState?> GetSyncStateAsync(string vaultId, string networkId, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM sync_state WHERE vault_id = $vault AND network_id = $network LIMIT 1";
        command.Parameters.AddWithValue("$vault", vaultId);
        command.Parameters.AddWithValue("$network", networkId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return new SyncState
        {
            VaultId = reader.GetString(0),
            NetworkId = reader.GetString(1),
            LastProcessedBlock = reader.GetInt64(2),
            LastProcessedTransactionHash = reader.IsDBNull(3) ? null : reader.GetString(3),
            LastSyncStartedAt = reader.GetDateTime(4),
            LastSyncCompletedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
            CurrentPhase = (SyncPhase)reader.GetInt32(6),
            AccountsDiscovered = reader.GetInt32(7),
            TransactionsIndexed = reader.GetInt32(8),
            LastErrorMessage = reader.IsDBNull(9) ? null : reader.GetString(9)
        };
    }

    public async Task CacheTransactionsAsync(string vaultId, IEnumerable<TransactionRecord> transactions, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        foreach (var tx in transactions)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT OR IGNORE INTO transactions
                (tx_hash, vault_id, network_id, from_addr, to_addr, value, asset, block_number, confirmations, direction, status, fee, timestamp)
                VALUES ($hash, $vault, $network, $from, $to, $value, $asset, $block, $conf, $dir, $status, $fee, $ts)
                """;
            command.Parameters.AddWithValue("$hash", tx.TransactionHash);
            command.Parameters.AddWithValue("$vault", vaultId);
            command.Parameters.AddWithValue("$network", tx.NetworkId);
            command.Parameters.AddWithValue("$from", tx.FromAddress);
            command.Parameters.AddWithValue("$to", tx.ToAddress);
            command.Parameters.AddWithValue("$value", tx.Value);
            command.Parameters.AddWithValue("$asset", tx.AssetSymbol);
            command.Parameters.AddWithValue("$block", tx.BlockNumber);
            command.Parameters.AddWithValue("$conf", tx.Confirmations);
            command.Parameters.AddWithValue("$dir", (int)tx.Direction);
            command.Parameters.AddWithValue("$status", (int)tx.Status);
            command.Parameters.AddWithValue("$fee", (object?)tx.FeePaid ?? DBNull.Value);
            command.Parameters.AddWithValue("$ts", tx.Timestamp.UtcDateTime);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<IReadOnlyList<TransactionRecord>> GetTransactionsAsync(string vaultId, string? networkId, int limit, CancellationToken cancellationToken)
    {
        var records = new List<TransactionRecord>();
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT tx_hash, network_id, from_addr, to_addr, value, asset, block_number, confirmations, direction, status, fee, timestamp
            FROM transactions
            WHERE vault_id = $vault
            ORDER BY timestamp DESC
            LIMIT $limit
            """;
        command.Parameters.AddWithValue("$vault", vaultId);
        command.Parameters.AddWithValue("$limit", limit);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            records.Add(new TransactionRecord
            {
                TransactionHash = reader.GetString(0),
                NetworkId = reader.GetString(1),
                FromAddress = reader.GetString(2),
                ToAddress = reader.GetString(3),
                Value = reader.GetDecimal(4),
                AssetSymbol = reader.GetString(5),
                BlockNumber = reader.GetInt64(6),
                Confirmations = reader.GetInt64(7),
                Direction = (TransactionDirection)reader.GetInt32(8),
                Status = (TransactionStatus)reader.GetInt32(9),
                FeePaid = reader.IsDBNull(10) ? null : reader.GetDecimal(10),
                Timestamp = reader.GetDateTime(11)
            });
        }

        return records;
    }
}
