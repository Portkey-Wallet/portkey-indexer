using GraphQL;

namespace Portkey.Indexer.CA.GraphQL;

public class LoginGuardianAccountChangeRecordDto : LoginGuardianAccountDtoBase
{
    public string ChangeType { get; set; }

    public long BlockHeight { get; set; }
}