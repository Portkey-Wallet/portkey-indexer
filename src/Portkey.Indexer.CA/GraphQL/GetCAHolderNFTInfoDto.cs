using System.ComponentModel;
using GraphQL;
using Volo.Abp.Application.Dtos;

namespace Portkey.Indexer.CA.GraphQL;

public class GetCAHolderNFTInfoDto: PagedResultRequestDto
{
    public string ChainId { get; set; }
    public string Symbol { get; set; }
    [Name("caAddresses")]
    public List<string> CAAddresses { get; set; }
    public string CollectionSymbol { get; set; }
    
}