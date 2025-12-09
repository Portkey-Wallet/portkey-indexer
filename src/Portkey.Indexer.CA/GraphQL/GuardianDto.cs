namespace Portkey.Indexer.CA.GraphQL;

public class GuardianDto
{
    // public GuardianDto Guardian { get; set; }
    public int Type { get; set; }
    public string VerifierId { get; set; }
    public string IdentifierHash { get; set; }
    public string Salt { get; set; }
    public bool IsLoginGuardian { get; set; }
}

// public class GuardianDto
// {
//     public int Type { get; set; }
//     public string Verifier { get; set; }
// }