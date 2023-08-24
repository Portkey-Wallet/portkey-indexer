using GraphQL;
using Volo.Abp.Application.Dtos;

namespace Portkey.Indexer.CA.GraphQL;

public class GetGuardianAddedCAHolderInfo : PagedResultRequestDto
{
    [Name("loginGuardianIdentifierHash")] public string LoginGuardianIdentifierHash { get; set; }
}