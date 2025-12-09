namespace Portkey.Indexer.CA.GraphQL;

public class ReferralInfoDto
{
    public string CaHash { get; set; }
    public string MethodName { get; set; }
    public string ReferralCode { get; set; }
    public string ProjectCode { get; set; }
    public long Timestamp { get; set; }
}