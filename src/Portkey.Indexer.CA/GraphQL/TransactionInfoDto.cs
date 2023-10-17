namespace Portkey.Indexer.CA.GraphQL;

public class TransactionInfoDto
{
    public string TransactionId { get; set; }
    public long BlockHeight { get; set; }
    public string MethodName { get; set; }
    public bool Confirmed { get; set; }
}