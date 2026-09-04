using Cs2Hvh.Engine.Analytics;
using Cs2Hvh.Engine.Domain.Models;
using Xunit;

namespace Cs2Hvh.Engine.Tests;

public class TransactionPipelineTests
{
    [Fact]
    public void FilterByDirection_ReturnsMatchingRecords()
    {
        var pipeline = new TransactionPipeline();
        var records = new[]
        {
            CreateTx(TransactionDirection.Incoming),
            CreateTx(TransactionDirection.Outgoing),
            CreateTx(TransactionDirection.Incoming)
        };

        var incoming = pipeline.FilterByDirection(records, TransactionDirection.Incoming);
        Assert.Equal(2, incoming.Count);
    }

    [Fact]
    public void SumIncomingValue_AggregatesCorrectly()
    {
        var pipeline = new TransactionPipeline();
        var records = new[]
        {
            CreateTx(TransactionDirection.Incoming, 1.5m),
            CreateTx(TransactionDirection.Incoming, 2.5m),
            CreateTx(TransactionDirection.Outgoing, 9m)
        };

        var total = pipeline.SumIncomingValue(records, "ETH");
        Assert.Equal(4m, total);
    }

    private static TransactionRecord CreateTx(TransactionDirection direction, decimal value = 1m) =>
        new()
        {
            TransactionHash = Guid.NewGuid().ToString("N"),
            NetworkId = "ethereum-mainnet",
            FromAddress = "0xabc",
            ToAddress = "0xdef",
            Value = value,
            AssetSymbol = "ETH",
            BlockNumber = 100,
            Confirmations = 12,
            Direction = direction,
            Status = TransactionStatus.Confirmed,
            Timestamp = DateTimeOffset.UtcNow
        };
}
