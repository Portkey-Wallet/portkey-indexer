using GraphQL;

namespace Portkey.Indexer.CA.GraphQL;

public class UserNFTInfoDto
{
    public string Id { get; set; }
    public string ChainId { get; set; }
    [Name("caAddress")]
    public string CAAddress { get; set; }
    public long Balance { get; set; }
    public NFTItemInfoDto NftInfo { get; set; }
}
