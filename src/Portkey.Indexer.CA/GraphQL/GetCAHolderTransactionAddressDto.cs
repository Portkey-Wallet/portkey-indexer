using GraphQL;
using Volo.Abp.Application.Dtos;

namespace Portkey.Indexer.CA.GraphQL;

public class GetCAHolderTransactionAddressDto : PagedResultRequestDto
{
    public string ChainId { get; set; }
    
    [Name("caAddressInfos")]
    public List<CAAddressInfo> CAAddressInfos { get; set; }
}