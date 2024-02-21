using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class CompatibleCrossChainTransferIndex : AElfIndexerClientEntity<string>, IIndexBuild
{
    [Keyword] public override string Id { get; set; }
    [Keyword] public string TransactionId { get; set; }
    public long Timestamp { get; set; }
    [Keyword]  public string FromAddress { get; set; }
    [Keyword]  public string ToAddress { get; set; }
}