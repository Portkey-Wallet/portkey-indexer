using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class CAHolderNFTBalanceIndex: AElfIndexerClientEntity<string>, IIndexBuild
{
    public NFTInfoIndex NftInfo { get; set; }
    [Keyword]public string CAAddress { get; set; }
    
    [Keyword] public long Balance { get; set; }
}