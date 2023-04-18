using AElfIndexer.Client;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class NFTInfoBase: AElfIndexerClientEntity<string>
{
    [Keyword] public override string Id { get; set; }

    [Keyword] public string ProtocolName { get; set; }
    
    [Keyword]public string Symbol { get; set; }
    
    public long TokenId { get; set; }
    
    /// <summary>
    /// NFT contract address
    /// </summary>
    [Keyword] public string NftContractAddress { get; set; }
    
    [Keyword] public string Owner { get; set; }

    [Keyword] public string Minter { get; set; }

    [Keyword] public long Quantity { get; set; }
    
    [Keyword] public string Alias { get; set; }

    [Keyword] public string BaseUri { get; set; }

    [Keyword] public string Uri { get; set; }

    [Keyword] public string Creator { get; set; }

    [Keyword] public string NftType { get; set; }
    
    public long TotalQuantity { get; set; }
    
    [Keyword] public string TokenHash { get; set; }
    
    [Keyword] public string ImageUrl { get; set; }
}