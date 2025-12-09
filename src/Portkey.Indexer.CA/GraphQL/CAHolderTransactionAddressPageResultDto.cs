namespace Portkey.Indexer.CA.GraphQL;

public class CAHolderTransactionAddressPageResultDto
{
    public long TotalRecordCount { get; set; }
    
    public List<CAHolderTransactionAddressDto> Data { get; set; }
}