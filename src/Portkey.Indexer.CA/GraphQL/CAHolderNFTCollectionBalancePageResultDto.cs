namespace Portkey.Indexer.CA.GraphQL;

public class CAHolderNFTCollectionBalancePageResultDto
{
    public long TotalRecordCount { get; set; }
    
    public List<CAHolderNFTCollectionBalanceInfoDto> Data { get; set; }
}