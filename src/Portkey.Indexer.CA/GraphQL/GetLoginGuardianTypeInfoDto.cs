using GraphQL;
using Volo.Abp.Application.Dtos;

namespace Portkey.Indexer.CA.GraphQL;

public class GetLoginGuardianAccountInfoDto : PagedResultRequestDto
{
    public string ChainId { get; set; }
    
    [Name("caHash")]
    public string CAHash { get; set; }
    
    [Name("caAddress")]
    public string CAAddress { get; set; }
    
    public string LoginGuardianAccount { get; set; }
}