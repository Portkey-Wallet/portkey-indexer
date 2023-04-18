using GraphQL;

namespace Portkey.Indexer.CA.GraphQL;

public class GetLoginGuardianAccountChangeRecordDto
{
    public string ChainId { get; set; }

    public long StartBlockHeight { get; set; }
    
    public long EndBlockHeight { get; set; }
}