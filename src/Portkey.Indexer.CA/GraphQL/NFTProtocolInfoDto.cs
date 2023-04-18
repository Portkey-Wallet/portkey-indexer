namespace Portkey.Indexer.CA.GraphQL;

public class NFTProtocolInfoDto
{
    public string Id { get; set; }
    
    public string ChainId { get; set; }

    public string BlockHash { get; set; }

    public long BlockHeight { get; set; }

    public string PreviousBlockHash { get; set; }

    public string ProtocolName { get; set; }
    
    public string Symbol { get; set; }
    
    public long TokenId { get; set; }

    public string Owner { get; set; }

    public string Minter { get; set; }

    public long Quantity { get; set; }

    public string Alias { get; set; }

    public string BaseUri { get; set; }

    public string Uri { get; set; }
    public string Creator { get; set; }
    public string NftType { get; set; }
    
    public long TotalQuantity { get; set; }
    
    public string TokenHash { get; set; }
    
    public string ImageUrl { get; set; }
}