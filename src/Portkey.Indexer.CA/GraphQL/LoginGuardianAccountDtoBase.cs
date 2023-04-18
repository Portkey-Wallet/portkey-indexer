using GraphQL;

namespace Portkey.Indexer.CA.GraphQL;

public class LoginGuardianAccountDtoBase
{
    public string Id { get; set; }
    public string ChainId { get; set; }
    [Name("caHash")]
    public string CAHash { get; set; }
    [Name("caAddress")]
    public string CAAddress { get; set; }
    public string Manager { get; set; }
    public GuardianAccountDto LoginGuardianAccount { get; set; }
}