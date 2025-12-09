namespace Portkey.Indexer.CA.GraphQL;

public class NFTSearchInfoDto
{
    public string ProtocolName { get; set; }
    
    public string Symbol { get; set; }
    
    public long TokenId { get; set; }
    
    public string NftContractAddress { get; set; }
    
    public string Owner { get; set; }
    
    public string Minter { get; set; }
    
    public long Quantity { get; set; }
    
    public string Alias { get; set; }
    
    public string BaseUri { get; set; }
    
    public string Uri { get; set; }
}