namespace Portkey.Indexer.CA.GraphQL;

public class CAHolderNFTBalancePageResultDto
{
    public long TotalRecordCount { get; set; }
    
    public List<CAHolderNFTBalanceInfoDto> Data { get; set; }
}