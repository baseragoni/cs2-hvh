namespace Cs2Hvh.ChainProviders.Resilience;

/// <summary>
/// Exponential backoff retry policy for transient JSON-RPC failures.
/// </summary>
public sealed class RpcRetryPolicy
{
    private readonly int _maxAttempts;
    private readonly TimeSpan _initialDelay;

    public RpcRetryPolicy(int maxAttempts = 5, TimeSpan? initialDelay = null)
    {
        _maxAttempts = maxAttempts;
        _initialDelay = initialDelay ?? TimeSpan.FromMilliseconds(250);
    }

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken)
    {
        var delay = _initialDelay;
        Exception? lastException = null;

        for (var attempt = 1; attempt <= _maxAttempts; attempt++)
        {
            try
            {
                return await action(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (attempt < _maxAttempts && IsTransient(ex))
            {
                lastException = ex;
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                delay += delay;
            }
        }

        throw lastException ?? new InvalidOperationException("RPC retry policy failed without exception.");
    }

    private static bool IsTransient(Exception ex) =>
        ex is HttpRequestException or TaskCanceledException or InvalidOperationException;
}
