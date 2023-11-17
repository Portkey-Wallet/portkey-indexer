using Volo.Abp.Application.Dtos;

namespace Portkey.Indexer.CA.GraphQL;

public class GetCaHolderBalanceChangeRecordDto : PagedResultRequestDto
{
    public string ChainId { get; set; }
    public string CaHash { get; set; }
    public string CaAddress { get; set; }
}