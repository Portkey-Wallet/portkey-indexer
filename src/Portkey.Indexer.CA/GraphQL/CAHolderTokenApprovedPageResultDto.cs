namespace Portkey.Indexer.CA.GraphQL;

public class CAHolderTokenApprovedPageResultDto
{
    public long TotalRecordCount { get; set; }
    public List<CAHolderTokenApprovedDto> Data { get; set; }
}