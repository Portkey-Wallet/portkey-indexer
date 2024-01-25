using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class InviteIndex : AElfIndexerClientEntity<string>, IIndexBuild
{
    [Keyword] public override string Id { get; set; }
    [Keyword] public string CaHash { get; set; }
    [Keyword] public string MethodName { get; set; }
    [Keyword] public string ReferralCode { get; set; }
    [Keyword] public string ProjectCode { get; set; }
    public long Timestamp { get; set; }
}