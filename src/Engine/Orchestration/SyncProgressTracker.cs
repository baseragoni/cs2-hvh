using Cs2Hvh.Engine.Domain.Models;

namespace Cs2Hvh.Engine.Orchestration;

/// <summary>
/// Emits granular sync progress events for CLI and host integrations.
/// </summary>
public sealed class SyncProgressTracker
{
    public event Action<SyncProgressEvent>? ProgressChanged;

    public void Report(SyncPhase phase, string networkId, int current, int total, string? detail = null)
    {
        var evt = new SyncProgressEvent
        {
            Phase = phase,
            NetworkId = networkId,
            Current = current,
            Total = total,
            Detail = detail,
            Timestamp = DateTimeOffset.UtcNow,
            PercentComplete = total <= 0 ? 0 : (int)((double)current / total * 100)
        };

        ProgressChanged?.Invoke(evt);
    }

    public void ReportCompleted(string networkId, SyncState finalState) =>
        Report(SyncPhase.Completed, networkId, finalState.TransactionsIndexed, finalState.TransactionsIndexed,
            $"block={finalState.LastProcessedBlock}");
}

public sealed class SyncProgressEvent
{
    public SyncPhase Phase { get; init; }
    public required string NetworkId { get; init; }
    public int Current { get; init; }
    public int Total { get; init; }
    public int PercentComplete { get; init; }
    public string? Detail { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}
