using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class CAHolderTokenApprovedIndex : AElfIndexerClientEntity<string>, IIndexBuild
{
    [Keyword] public override string Id { get; set; }

    [Keyword] public string CAAddress { get; set; }
    [Keyword] public string Spender { get; set; }
    [Keyword] public string Symbol { get; set; }
    public long BatchApprovedAmount { get; set; }
}