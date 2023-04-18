using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class CAHolderNFTCollectionBalanceIndex: AElfIndexerClientEntity<string>,IIndexBuild
{
    public NFTCollectionInfoIndex NftCollectionInfo { get; set; }
    [Keyword]public string CAAddress { get; set; }
    
    [Keyword] public long Balance { get; set; }
    [Keyword]public List<long> TokenIds { get; set; }
}