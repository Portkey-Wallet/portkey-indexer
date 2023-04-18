using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class CAHolderSearchTokenNFTIndex: AElfIndexerClientEntity<string>, IIndexBuild
{
    [Keyword]public string CAAddress { get; set; }
    
    public long Balance { get; set; }
    
    public TokenSearchInfo TokenInfo { get; set; }
    
    public NFTSearchInfo NFTInfo { get; set; }
}

public class TokenSearchInfo
{
    [Wildcard]public string Symbol { get; set; }
    
    [Wildcard]public string TokenContractAddress { get; set; }
    
    public int Decimals { get; set; }
    
    public long TotalSupply { get; set; }
    
    [Wildcard]public string TokenName { get; set; }
    
    [Wildcard]public string Issuer { get; set; }
    
    public bool IsBurnable { get; set; }
    
    public int IssueChainId { get; set; }
}

public class NFTSearchInfo
{
    [Wildcard]public string ProtocolName { get; set; }
    
    [Wildcard]public string Symbol { get; set; }
    
    public long TokenId { get; set; }
    
    [Wildcard]public string NftContractAddress { get; set; }
    
    [Wildcard]public string Owner { get; set; }
    
    [Wildcard]public string Minter { get; set; }
    
    public long Quantity { get; set; }
    
    [Wildcard]public string Alias { get; set; }
    
    public string BaseUri { get; set; }
    
    public string Uri { get; set; }
}