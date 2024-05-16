namespace Portkey.Indexer.CA;

public class CAHolderTransactionInfoOptions
{
    public List<CAHolderTransactionInfo> CAHolderTransactionInfos { get; set; }
}

public class CAHolderTransactionInfo
{
    public string ChainId { get; set; }
    
    public string ContractAddress { get; set; }

    public string MethodName { get; set; }
    
    public List<string> EventNames { get; set; }
    public bool MultiTransaction { get; set; }
    public bool MultiTokenTransfer { get; set; }
}