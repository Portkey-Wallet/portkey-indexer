using GraphQL;

namespace Portkey.Indexer.CA.GraphQL;

public class CAHolderTokenBalanceDto
{
    public string ChainId { get; set; }
    
    [Name("caAddress")]
    public string CAAddress { get; set; }
    
    public TokenInfoDto TokenInfo { get; set; }
    
    public long Balance { get; set; }
    
    public List<long> TokenIds { get; set; }
}