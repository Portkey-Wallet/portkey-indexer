using AElfIndexer;

namespace Portkey.Indexer.CA.GraphQL;

public class GetSyncStateDto
{
    public string ChainId { get; set; }
    public BlockFilterType FilterType { get; set; }
}