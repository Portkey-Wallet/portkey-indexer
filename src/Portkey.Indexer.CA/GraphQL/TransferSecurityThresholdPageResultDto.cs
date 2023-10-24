namespace Portkey.Indexer.CA.GraphQL;

public class TransferSecurityThresholdPageResultDto
{
    public long TotalRecordCount { get; set; }

    public List<TransferSecurityThresholdDto> Data { get; set; }
}