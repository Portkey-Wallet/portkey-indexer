using Portkey.Indexer.CA.Entities;

namespace Portkey.Indexer.CA.GraphQL;

public class CaHolderBalanceChangeRecordPageResultDto
{
    public long TotalRecordCount { get; set; }
    
    public List<CaHolderBalanceChangeRecordDto> Data { get; set; }
}

public class CaHolderBalanceChangeRecordDto
{
    public string Id { get; set; }
    public string ChainId { get; set; }
    public string CaAddress { get; set; }
    public string TransactionId { get; set; }
    public long TransactionTime { get; set; }
    public OperatorType OperatorType { get; set; }
    public TokenBasicInfo TokenInfo { get; set; }
    public long Amount { get; set; }
}