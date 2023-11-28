namespace Portkey.Indexer.CA.GraphQL;

public class CAHolderTransferlimitDto
{
    public string ChainId { get; set; }
    public string CAHash { get; set; }
    public string Symbol { get; set; }
    public string SingleLimit { get; set; }
    public string DailyLimit { get; set; }
}