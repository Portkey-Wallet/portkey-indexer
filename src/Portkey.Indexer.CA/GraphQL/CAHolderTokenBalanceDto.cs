using GraphQL;

namespace Portkey.Indexer.CA.GraphQL;

public class CAHolderTokenBalanceDto
{
    public string ChainId { get; set; }
    
    [Name("caAddress")]
    public string CAAddress { get; set; }
    
    public Entities.TokenInfo TokenInfo { get; set; }
    
    public long Balance { get; set; }
}