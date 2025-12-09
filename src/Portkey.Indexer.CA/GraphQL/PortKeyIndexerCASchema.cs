using AElfIndexer.Client.GraphQL;

namespace Portkey.Indexer.CA.GraphQL;

public class PortKeyIndexerCASchema : AElfIndexerClientSchema<Query>
{
    public PortKeyIndexerCASchema(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}