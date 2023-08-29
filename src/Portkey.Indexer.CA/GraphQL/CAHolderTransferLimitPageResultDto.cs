namespace Portkey.Indexer.CA.GraphQL;

public class CAHolderTransferLimitPageResultDto
{
    public long TotalRecordCount { get; set; }

    public List<CAHolderTransferlimitDto> Data { get; set; }
}