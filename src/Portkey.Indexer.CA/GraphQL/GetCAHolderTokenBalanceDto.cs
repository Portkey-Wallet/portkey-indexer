using GraphQL;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.Application.Dtos;

namespace Portkey.Indexer.CA.GraphQL;

public class GetCAHolderTokenBalanceDto : PagedResultRequestDto
{
    public string ChainId { get; set; }
    
    [Name("caAddressInfos")]
    public List<CAAddressInfo> CAAddressInfos { get; set; }
    
    // public TokenType Type { get; set; }
    
    public string Symbol { get; set; }
}