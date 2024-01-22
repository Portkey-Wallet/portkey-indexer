namespace Portkey.Indexer.CA.GraphQL;

public class GetReferralInfoDto
{
    public string CaHash { get; set; }
    public List<string> MethodNames { get; set; }
    public string ReferralCode { get; set; }
    public string ProjectCode { get; set; }
}