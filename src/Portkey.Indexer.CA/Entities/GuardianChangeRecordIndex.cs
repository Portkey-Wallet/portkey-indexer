using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class GuardianChangeRecordIndex : AElfIndexerClientEntity<string>, IIndexBuild
{
    [Keyword] public override string Id { get; set; }

    [Keyword] public string CAHash { get; set; }
    [Keyword] public string CAAddress { get; set; }
    [Keyword] public string ChangeType { get; set; }

    public Guardian Guardian { get; set; }
}