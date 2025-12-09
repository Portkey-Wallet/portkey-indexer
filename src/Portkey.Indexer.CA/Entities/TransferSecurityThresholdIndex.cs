using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class TransferSecurityThresholdIndex : AElfIndexerClientEntity<string>, IIndexBuild
{
    [Keyword] public string Symbol { get; set; }
    public long BalanceThreshold { get; set; }
    public long GuardianThreshold { get; set; }
}