using GraphQL;

namespace Portkey.Indexer.CA.GraphQL;

public class LoginGuardianChangeRecordDto : LoginGuardianDtoBase
{
    public string ChangeType { get; set; }

    public long BlockHeight { get; set; }
}