namespace Portkey.Indexer.CA.GraphQL;

public class CAHolderManagerApprovedDto
{
    public string ChainId { get; set; }
    public string CAHash { get; set; }
    public string Spender { get; set; }
    public string Symbol { get; set; }
    public long Amount { get; set; }
    public long BlockHeight { get; set; }
}