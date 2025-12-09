using AElf.Contracts.MultiToken;
using Google.Protobuf.Collections;

namespace Portkey.Indexer.CA.Provider;

public class TokenInfoDto
{
    public string Symbol { get; set; }
    public string Issuer { get; set; }
    public long Supply { get; set; }
    public long TotalSupply { get; set; }
    public string TokenName { get; set; }
    public int Decimals { get; set; }
    public int IssueChainId { get; set; }
    public bool IsBurnable { get; set; }
    public long Issued { get; set; }
    public MapField<string, string> ExternalInfo { get; set; }
}