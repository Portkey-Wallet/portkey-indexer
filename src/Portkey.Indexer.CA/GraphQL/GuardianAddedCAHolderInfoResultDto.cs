namespace Portkey.Indexer.CA.GraphQL;

public class GuardianAddedCAHolderInfoResultDto
{
    public long TotalRecordCount { get; set; }
    public List<CAHolderInfoDto> Data { get; set; }
}