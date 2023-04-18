using System.ComponentModel;
using GraphQL;
using Volo.Abp.Application.Dtos;

namespace Portkey.Indexer.CA.GraphQL;

public class GetUserNFTInfoDto: PagedResultRequestDto
{
    public string ChainId { get; set; }
    public string Symbol { get; set; }
    [DefaultValue(0)]
    public long TokenId { get; set; }
    [Name("caAddresses")]
    public List<string> CAAddresses { get; set; }
    
}