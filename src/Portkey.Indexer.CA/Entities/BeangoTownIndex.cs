using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Portkey.Contracts.BeangoTownContract;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class BeangoTownIndex: AElfIndexerClientEntity<string>, IIndexBuild
{
    [Keyword] public override string Id { get; set; }
    [Keyword] public string CaAddress { get; set; }
    [Keyword] public string? SeasonId { get; set; }
    public long PlayBlockHeight { get; set; }
    public bool IsComplete { get; set; }
    public GridType GridType;
    public int GridNum;
    public int Score;
    public long BingoBlockHeight { get; set; }

    public TransactionInfoIndex? PlayTransactionInfo { get; set; }
    public TransactionInfoIndex? BingoTransactionInfo { get; set; }
}