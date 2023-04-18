using Portkey.Indexer.CA.Entities;

namespace Portkey.Indexer.CA.GraphQL;

public class TokenSearchInfoDto
{
    public string Symbol { get; set; }
    
    public TokenType Type { get; set; }
    
    public string TokenContractAddress { get; set; }
    
    public int Decimals { get; set; }
    
    public long Supply { get; set; }
    
    public long TotalSupply { get; set; }
    
    public string TokenName { get; set; }
    
    public string Issuer { get; set; }
    
    public bool IsBurnable { get; set; }
    
    public int IssueChainId { get; set; }
    
    public long TokenId { get; set; }
    
    // public string Alias { get; set; }
    
    public string ImageUrl { get; set; }
    
    // public TokenExternalInfo TokenExternalInfo { get; set; }
    
    // public TokenSearchInfoDto RelatedTokenInfo { get; set; }
}