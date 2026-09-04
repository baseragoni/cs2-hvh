namespace Cs2Hvh.Persistence.Stores;

public sealed class BackupSnapshotService
{
    public async Task CreateSnapshotAsync(string sourceDatabasePath, string destinationPath, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
        await using var source = File.OpenRead(sourceDatabasePath);
        await using var destination = File.Create(destinationPath);
        await source.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
    }
}
