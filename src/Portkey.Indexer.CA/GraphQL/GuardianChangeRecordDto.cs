namespace Portkey.Indexer.CA.GraphQL;

public class GuardianChangeRecordDto
{
    public string Id { get; set; }
    public string CAHash { get; set; }
    public string CAAddress { get; set; }
    public string GuardiansMerkleTreeRoot { get; set; }
    public string ChangeType { get; set; }
    public GuardianDto Guardian { get; set; }
}