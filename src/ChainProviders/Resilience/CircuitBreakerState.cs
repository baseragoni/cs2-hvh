namespace Cs2Hvh.ChainProviders.Resilience;

public enum CircuitState
{
    Closed = 0,
    Open = 1,
    HalfOpen = 2
}

/// <summary>
/// Tracks consecutive RPC failures and opens circuit to protect upstream nodes.
/// </summary>
public sealed class CircuitBreakerState
{
    private int _failureCount;
    private DateTimeOffset? _openedAt;

    public CircuitState State { get; private set; } = CircuitState.Closed;

    public int FailureThreshold { get; }

    public TimeSpan Cooldown { get; }

    public CircuitBreakerState(int failureThreshold = 5, TimeSpan? cooldown = null)
    {
        FailureThreshold = failureThreshold;
        Cooldown = cooldown ?? TimeSpan.FromSeconds(30);
    }

    public bool AllowRequest()
    {
        if (State == CircuitState.Open && _openedAt.HasValue)
        {
            if (DateTimeOffset.UtcNow - _openedAt.Value >= Cooldown)
            {
                State = CircuitState.HalfOpen;
                return true;
            }

            return false;
        }

        return true;
    }

    public void RecordSuccess()
    {
        _failureCount = 0;
        State = CircuitState.Closed;
        _openedAt = null;
    }

    public void RecordFailure()
    {
        _failureCount++;
        if (_failureCount >= FailureThreshold)
        {
            State = CircuitState.Open;
            _openedAt = DateTimeOffset.UtcNow;
        }
    }
}
