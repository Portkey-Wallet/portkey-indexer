using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class CAHolderSearchTokenNFTIndex: AElfIndexerClientEntity<string>, IIndexBuild
{
    [Keyword]public string CAAddress { get; set; }
    
    public long Balance { get; set; }
    
    public long TokenId { get; set; }
    
    // public TokenInfoIndex TokenInfo { get; set; }
    
    public TokenSearchInfo TokenInfo { get; set; }
    
    // public NFTInfoIndex NftInfo { get; set; }
    
    public NFTSearchInfo NftInfo { get; set; }
}

public class TokenSearchInfo
{
    [Wildcard]public string Symbol { get; set; }
    
    public TokenType Type { get; set; }
    [Wildcard]public string TokenContractAddress { get; set; }
    
    public int Decimals { get; set; }
    
    public long Supply { get; set; }
    
    public long TotalSupply { get; set; }
    
    [Wildcard]public string TokenName { get; set; }
    
    [Wildcard]public string Issuer { get; set; }
    
    public bool IsBurnable { get; set; }
    
    public int IssueChainId { get; set; }
    
    // public long TokenId { get; set; }
    
    // [Wildcard]public string Alias { get; set; }
    
    // public string ImageUrl { get; set; }
    
    // public TokenExternalInfo TokenExternalInfo { get; set; }

    // public Dictionary<string, string> ExternalInfoDictionary { get; set; }
    
    // public TokenInfoIndex RelatedTokenInfo { get; set; }
}

public class NFTSearchInfo
{
    [Wildcard]public string CollectionSymbol { get; set; }
    
    [Wildcard]public string CollectionName { get; set; }
    
    [Wildcard]public string Symbol { get; set; }
    
    public TokenType Type { get; set; }
    [Wildcard]public string TokenContractAddress { get; set; }
    
    public int Decimals { get; set; }
    
    public long Supply { get; set; }
    
    public long TotalSupply { get; set; }
    
    [Wildcard]public string TokenName { get; set; }
    
    [Wildcard]public string Issuer { get; set; }
    
    public bool IsBurnable { get; set; }
    
    public int IssueChainId { get; set; }

    // [Wildcard]public string Alias { get; set; }
    
    public string ImageUrl { get; set; }
}