using GraphQL;
using Volo.Abp.Application.Dtos;

namespace Portkey.Indexer.CA.GraphQL;

public class GetCAHolderManagerApprovedDto : PagedResultRequestDto
{
    public string ChainId { get; set; }
    [Name("caHash")] public string CAHash { get; set; }
    public string Spender { get; set; }
    public string Symbol { get; set; }
}