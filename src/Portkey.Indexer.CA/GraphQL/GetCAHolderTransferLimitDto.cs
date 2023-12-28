using GraphQL;

namespace Portkey.Indexer.CA.GraphQL;

public class GetCAHolderTransferLimitDto
{
    [Name("caHash")] public string CAHash { get; set; }
}