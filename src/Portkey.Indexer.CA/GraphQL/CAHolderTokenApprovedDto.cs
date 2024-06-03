using GraphQL;

namespace Portkey.Indexer.CA.GraphQL;

public class CAHolderTokenApprovedDto
{
    public string ChainId { get; set; }
    public string Spender { get; set; }
    [Name("caAddress")]
    public string CAAddress { get; set; }
    public string Symbol { get; set; }
    public long BatchApprovedAmount { get; set; }
}