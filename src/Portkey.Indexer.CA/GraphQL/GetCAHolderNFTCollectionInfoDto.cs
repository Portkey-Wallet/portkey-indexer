using GraphQL;
using Volo.Abp.Application.Dtos;

namespace Portkey.Indexer.CA.GraphQL;

public class GetCAHolderNFTCollectionInfoDto: PagedResultRequestDto
{
    public string ChainId { get; set; }
    public string Symbol { get; set; }
    
    [Name("caAddressInfos")]
    public List<CAAddressInfo> CAAddressInfos { get; set; }
}