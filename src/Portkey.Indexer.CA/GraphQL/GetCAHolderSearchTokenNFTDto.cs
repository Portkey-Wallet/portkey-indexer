using GraphQL;
using Volo.Abp.Application.Dtos;

namespace Portkey.Indexer.CA.GraphQL;

public class GetCAHolderSearchTokenNFTDto: PagedResultRequestDto
{
    public string ChainId { get; set; }
    [Name("caAddressInfos")]
    public List<CAAddressInfo> CAAddressInfos { get; set; }
    
    public string SearchWord { get; set; }
}