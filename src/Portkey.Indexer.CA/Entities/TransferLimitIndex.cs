using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;

namespace Portkey.Indexer.CA.Entities;

public class TransferLimitIndex : AElfIndexerClientEntity<string>, IIndexBuild
{
    public string CaHash { get; set; }
    public string Symbol { get; set; }
    public long SingleLimit { get; set; }
    public long DailyLimit { get; set; }
}