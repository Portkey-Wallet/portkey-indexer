using GraphQL;

namespace Portkey.Indexer.CA.GraphQL;

public class CAHolderSearchTokenNFTDto
{
    public string ChainId { get; set; }
    
    [Name("caAddress")]
    public string CAAddress { get; set; }
    
    public long Balance { get; set; }
    
    public TokenSearchInfoDto TokenInfo { get; set; }
    
    [Name("nftInfo")]
    public NFTSearchInfoDto NFTInfo { get; set; }
}