using GraphQL;

namespace Portkey.Indexer.CA.GraphQL;

public class GetCAHolderManagerChangeRecordDto
{
    public string ChainId { get; set; }
    
    // [Name("caHash")]
    // public string CAHash { get; set; }
    
    public long StartBlockHeight { get; set; }
    
    public long EndBlockHeight { get; set; }
}