using Volo.Abp.Application.Dtos;

namespace Portkey.Indexer.CA.GraphQL;

public class GetNftItemInfosDto : PagedResultRequestDto
{
    public List<GetNftItemInfo> GetNftItemInfos { get; set; }
}

public class GetNftItemInfo
{
    public string Symbol { get; set; }
    public string ChainId { get; set; }
    
    public string CollectionSymbol { get; set; }
}