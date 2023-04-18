using GraphQL;

namespace Portkey.Indexer.CA.GraphQL;

public class UserNFTProtocolInfoDto
{
    public string Id { get; set; }
    public string ChainId { get; set; }
    [Name("caAddress")]
    public string CAAddress { get; set; }
    public List<long> TokenIds { get; set; }
    public NFTProtocolDto NftProtocolInfo { get; set; }
}
