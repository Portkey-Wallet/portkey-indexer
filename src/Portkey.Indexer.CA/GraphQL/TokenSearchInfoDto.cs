namespace Portkey.Indexer.CA.GraphQL;

public class TokenSearchInfoDto
{
    public string Symbol { get; set; }
    
    public string TokenContractAddress { get; set; }
    
    public int Decimals { get; set; }
    
    public long TotalSupply { get; set; }
    
    public string TokenName { get; set; }
    
    public string Issuer { get; set; }
    
    public bool IsBurnable { get; set; }
    
    public int IssueChainId { get; set; }
}