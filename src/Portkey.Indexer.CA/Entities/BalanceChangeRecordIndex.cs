using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class BalanceChangeRecordIndex : AElfIndexerClientEntity<string>, IIndexBuild
{
    [Keyword] public override string Id { get; set; }
    [Keyword] public string ChainId { get; set; }
    [Keyword] public string CaAddress { get; set; }
    [Keyword] public string TransactionId { get; set; }
    public long TransactionTime { get; set; }
    [Keyword] public string OperatorType { get; set; }
    public TokenBasicInfo TokenInfo { get; set; }
    [Keyword] public string BalanceChangeType { get; set; }
    public long Amount { get; set; }
}

public enum OperatorType
{
    Add,
    Minus
}

public enum BalanceChangeType
{
    TokenBurned,
    TokenCrossChainReceived,
    TokenIssued,
    TokenTransferred,
    TransactionFeeCharged
}

public class TokenBasicInfo
{
    [Keyword] public string Symbol { get; set; }
    [Keyword] public string ChainId { get; set; }
}