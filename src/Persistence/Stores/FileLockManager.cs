namespace Cs2Hvh.Persistence.Stores;

public sealed class FileLockManager : IDisposable
{
    private readonly FileStream? _lockStream;

    public FileLockManager(string lockFilePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(lockFilePath)!);
        _lockStream = new FileStream(lockFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
    }

    public void Dispose() => _lockStream?.Dispose();
}
