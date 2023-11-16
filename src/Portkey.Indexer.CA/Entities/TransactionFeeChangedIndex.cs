using AElf.Indexing.Elasticsearch;
using AElf.Types;
using AElfIndexer.Client;

namespace Portkey.Indexer.CA.Entities;

public class TransactionFeeChangedIndex : AElfIndexerClientEntity<string>, IIndexBuild
{
    public string CAAddress { get; set; }
    public string Symbol { get; set; }
    public long Amount { get; set; }
    public string TransactionId { get; set; }
    public string ConsumerAddress { get; set; }
}