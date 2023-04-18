using GraphQL;
using Portkey.Indexer.CA.Entities;

namespace Portkey.Indexer.CA.GraphQL;

public class CAHolderManagerDto
{
    public string Id { get; set; }
    public string ChainId { get; set; }
    [Name("caHash")]
    public string CAHash { get; set; }
    [Name("caAddress")]
    public string CAAddress { get; set; }
    public List<ManagerInfo> ManagerInfos { get; set; }
}