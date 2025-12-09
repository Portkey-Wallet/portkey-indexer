using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class CAHolderTokenBalanceIndex : AElfIndexerClientEntity<string>, IIndexBuild
{
    [Keyword]
    public string CAAddress { get; set; }

    public TokenInfoIndex TokenInfo { get; set; }
    
    public long Balance { get; set; }
}