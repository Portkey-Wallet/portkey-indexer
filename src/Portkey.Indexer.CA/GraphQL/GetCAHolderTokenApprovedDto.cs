using GraphQL;
using Volo.Abp.Application.Dtos;

namespace Portkey.Indexer.CA.GraphQL;

public class GetCAHolderTokenApprovedDto : PagedResultRequestDto
{
    public string ChainId { get; set; }
    
    [Name("caAddresses")] public List<string> CAAddresses { get; set; }
}