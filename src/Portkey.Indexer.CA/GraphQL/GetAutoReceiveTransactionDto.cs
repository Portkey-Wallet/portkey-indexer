using Volo.Abp.Application.Dtos;

namespace Portkey.Indexer.CA.GraphQL;

public class GetAutoReceiveTransactionDto : PagedResultRequestDto
{
    public List<string> TransferTransactionIds { get; set; }
}