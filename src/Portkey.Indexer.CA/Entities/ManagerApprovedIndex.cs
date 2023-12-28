using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class ManagerApprovedIndex : AElfIndexerClientEntity<string>, IIndexBuild
{
    [Keyword] public override string Id { get; set; }

    [Keyword] public string Symbol { get; set; }
    [Keyword] public string CaHash { get; set; }
    [Keyword] public string Spender { get; set; }
    public long Amount { get; set; }
}