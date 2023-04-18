using GraphQL;
using Volo.Abp.Application.Dtos;

namespace Portkey.Indexer.CA.GraphQL;

public class GetCAHolderManagerInfoDto: PagedResultRequestDto
{
    public string ChainId { get; set; }
    
    [Name("caHash")]
    public string CAHash { get; set; }
    
    [Name("caAddresses")]
    public List<string> CAAddresses { get; set; }
    
    public string Manager { get; set; }
}