namespace Portkey.Indexer.CA.GraphQL;

public class GetTransactionIdsDto
{
    public string ChainId { get; set; }
    public List<string> TransactionList { get; set; }
    public bool? Confirmed { get; set; }
}