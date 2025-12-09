namespace Portkey.Indexer.CA.GraphQL;

public class CAHolderManagerApprovedPageResultDto
{
    public long TotalRecordCount { get; set; }

    public List<CAHolderManagerApprovedDto> Data { get; set; }
}