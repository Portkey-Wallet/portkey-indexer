using Volo.Abp.Application.Dtos;

namespace Portkey.Indexer.CA.GraphQL;

public class GetTokenInfoDto : PagedResultRequestDto
{
    public string Symbol { get; set; }
    public string ChainId { get; set; }
    
}