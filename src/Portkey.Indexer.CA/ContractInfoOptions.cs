namespace Portkey.Indexer.CA;

public class ContractInfoOptions
{
    public List<ContractInfo> ContractInfos { get; set; }
    public List<CATransactionInfo> CATransactionInfos { get; set; }
}

public class ContractInfo
{
    public string ChainId { get; set; }
    public string GenesisContractAddress { get; set; }
    public string CAContractAddress { get; set; }
    public string BingoGameContractAddress { get; set; }
    public string BeangoTownContractAddress { get; set; }
    public string TokenContractAddress { get; set; }
    
    public string NFTContractAddress { get; set; }
}

public class CATransactionInfo
{
    public string ChainId { get; set; }
    
    public string ContractAddress { get; set; }

    public string MethodName { get; set; }
    
    public List<string> BlackSubMethodNames { get; set; }
}