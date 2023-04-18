using Volo.Abp.Application.Dtos;

namespace Portkey.Indexer.CA.GraphQL;

public class GetNFTProtocolInfoDto : PagedResultRequestDto
{
    public string Symbol { get; set; }
    public string ChainId { get; set; }
    
}