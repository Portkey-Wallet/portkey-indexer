using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class UserNFTProtocolInfoIndex: AElfIndexerClientEntity<string>,IIndexBuild
{
    public NFTProtocol NftProtocolInfo { get; set; }
    [Keyword]public string CAAddress { get; set; }
    
    [Keyword]public List<long> TokenIds { get; set; }
}