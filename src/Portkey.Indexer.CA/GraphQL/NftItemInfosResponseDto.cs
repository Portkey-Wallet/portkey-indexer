using Portkey.Indexer.CA.Entities;

namespace Portkey.Indexer.CA.GraphQL;

public class NftItemInfosResponseDto
{
    public string Id { get; set; }
    public string ChainId { get; set; }
    public string Symbol { get; set; }
    public TokenType Type { get; set; }
    public string TokenContractAddress { get; set; }
    public int Decimals { get; set; }
    public long Supply { get; set; }
    public long TotalSupply { get; set; }
    public string TokenName { get; set; }
    public string Issuer { get; set; }
    public bool IsBurnable { get; set; }
    public int IssueChainId { get; set; }
    public Dictionary<string, string> ExternalInfoDictionary { get; set; } = new();
    public string ImageUrl { get; set; }
    public string CollectionSymbol { get; set; }
    public string CollectionName { get; set; }
}