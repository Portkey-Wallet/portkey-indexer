using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class UserNFTInfoIndex: AElfIndexerClientEntity<string>, IIndexBuild
{
    public NFTItemInfo NftInfo { get; set; }
    [Keyword]public string CAAddress { get; set; }
    
    [Keyword] public long Balance { get; set; }
}