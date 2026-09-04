namespace Cs2Hvh.ChainProviders.Transport;

public sealed class RateLimitHandler
{
    private readonly SemaphoreSlim _semaphore;
    private readonly Dictionary<string, DateTimeOffset> _lastRequest = new(StringComparer.OrdinalIgnoreCase);
    private readonly TimeSpan _minimumInterval;

    public RateLimitHandler(int maxConcurrent = 4, TimeSpan? minimumInterval = null)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
        _minimumInterval = minimumInterval ?? TimeSpan.FromMilliseconds(100);
    }

    public async Task WaitForSlotAsync(string networkId, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_lastRequest.TryGetValue(networkId, out var last))
            {
                var elapsed = DateTimeOffset.UtcNow - last;
                if (elapsed < _minimumInterval)
                {
                    await Task.Delay(_minimumInterval - elapsed, cancellationToken).ConfigureAwait(false);
                }
            }

            _lastRequest[networkId] = DateTimeOffset.UtcNow;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
