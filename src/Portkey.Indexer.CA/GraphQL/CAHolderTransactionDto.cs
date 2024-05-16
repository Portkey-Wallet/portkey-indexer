using GraphQL;
using Portkey.Indexer.CA.Entities;

namespace Portkey.Indexer.CA.GraphQL;

public class CAHolderTransactionDto
{
    public string Id { get; set; }
    
    public string ChainId { get; set; }

    public string BlockHash { get; set; }

    public long BlockHeight { get; set; }

    public string PreviousBlockHash { get; set; }
    
    public string TransactionId { get; set; }
    /// <summary>
    /// Method name
    /// </summary>
    public string MethodName { get; set; }
      
    public TokenInfoDto TokenInfo { get; set; }
    
    public NFTItemInfoDto NftInfo { get; set; }

    // [Name("nftInfo")]
    // public NFTInfo NFTInfo { get; set; }
      
    public TransactionStatus Status { get; set; }

    public long Timestamp { get; set; }

    public TransferInfo TransferInfo { get; set; }
    
    public List<TokenTransferInfo> TokenTransferInfos { get; set; }
    
    public string FromAddress { get; set; }

    public string ToContractAddress { get; set; }

    public List<TransactionFee> TransactionFees { get; set; }
    public bool IsManagerConsumer { get; set; } = false;
}

public class TransactionFee
{
    public string Symbol { get; set; }
    
    public long Amount { get; set; }
}