namespace Portkey.Indexer.CA.GraphQL;

public class CAHolderTransferLimitResultDto
{
    public long TotalRecordCount { get; set; }

    public List<CAHolderTransferlimitDto> Data { get; set; }
}