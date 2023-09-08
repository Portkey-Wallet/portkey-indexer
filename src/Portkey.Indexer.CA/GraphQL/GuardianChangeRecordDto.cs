using GraphQL;

namespace Portkey.Indexer.CA.GraphQL;

public class GuardianChangeRecordDto
{
    public string Id { get; set; }
    
    [Name("caHash")]
    public string CAHash { get; set; }
    
    [Name("caAddress")]
    public string CAAddress { get; set; }
    public string GuardiansMerkleTreeRoot { get; set; }
    public string ChangeType { get; set; }
    public GuardianDto Guardian { get; set; }
    
    public long BlockHeight { get; set; }
    public string BlockHash { get; set; }
}