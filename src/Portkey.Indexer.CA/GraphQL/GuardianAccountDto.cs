namespace Portkey.Indexer.CA.GraphQL;

public class GuardianAccountDto
{
    public GuardianDto Guardian { get; set; }
    public string Value { get; set; }
}

public class GuardianDto
{
    public int Type { get; set; }
    public string Verifier { get; set; }
}