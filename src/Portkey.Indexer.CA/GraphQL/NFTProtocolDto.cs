namespace Portkey.Indexer.CA.GraphQL;

public class NFTProtocolDto
{
    public string Id { get; set; }
    
    public string Symbol { get; set; }
    
    public string Creator { get; set; }
    
    public string NftType { get; set; }

    public string ProtocolName { get; set; }
    
    public string BaseUri { get; set; }
    
    public bool IsTokenIdReuse { get; set; }

    public long Supply { get; set; }
    
    public long TotalSupply { get; set; }
    
    public int IssueChainId { get; set; }
    
    public bool IsBurnable { get; set; }
  
    public string ImageUrl { get; set; }
}