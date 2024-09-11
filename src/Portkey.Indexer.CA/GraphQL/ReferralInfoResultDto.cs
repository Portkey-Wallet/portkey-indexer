namespace Portkey.Indexer.CA.GraphQL;

public class ReferralInfoResultDto
{
    public long TotalRecordCount { get; set; }
    public List<ReferralInfoDto> Data { get; set; }
}