namespace Portkey.Indexer.CA.GraphQL;

public class CAHolderSearchTokenNFTPageResultDto
{
    public long TotalRecordCount { get; set; }
    
    public List<CAHolderSearchTokenNFTDto> Data { get; set; }
}