namespace Portkey.Indexer.CA.GraphQL;

public class GetReferralInfoDto
{
    public List<string> CaHashes { get; set; }
    public List<string> MethodNames { get; set; }
    public List<string> ReferralCodes { get; set; }
    public string ProjectCode { get; set; }

    public long StartTime { get; set; }

    public long EndTime { get; set; }
}