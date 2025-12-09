namespace Portkey.Indexer.CA.GraphQL;

public class TransferSecurityThresholdDto
{
    public string Symbol { get; set; }
    public long BalanceThreshold { get; set; }
    public long GuardianThreshold { get; set; }
}