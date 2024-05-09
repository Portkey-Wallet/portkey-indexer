namespace Portkey.Indexer.CA.GraphQL;

public class CAHolderTokenApprovedDto
{
    public string ChainId { get; set; }
    public string Spender { get; set; }
    public string CAAddress { get; set; }
    public long BatchApprovedAmount { get; set; }
}