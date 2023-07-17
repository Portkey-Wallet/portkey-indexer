using GraphQL;
using Nest;
using Portkey.Indexer.CA.GraphQL;
using Volo.Abp.Application.Dtos;

namespace Portkey.Indexer.CA.Entities;

public class GetBingoDto: PagedResultRequestDto
{
    
    [Name("caAddresses")]
    public List<string> CAAddresses { get; set; }
    
    public string PlayId { get; set; }
}
