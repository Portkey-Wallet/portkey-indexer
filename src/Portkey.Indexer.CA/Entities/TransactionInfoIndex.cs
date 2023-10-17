using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class TransactionInfoIndex : AElfIndexerClientEntity<string>, IIndexBuild
{
    [Keyword] public string TransactionId { get; set; }
    [Keyword] public string MethodName { get; set; }
    public bool Confirmed { get; set; }
}