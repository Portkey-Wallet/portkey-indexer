using AElf;
using AElf.Types;
using Google.Protobuf.Collections;
using Nethereum.Hex.HexConvertors.Extensions;
using Portkey.Indexer.CA.Provider;

namespace Portkey.Indexer.CA.Tests.Provider;

public class MockAElfDataProvider : IAElfDataProvider
{
    public async Task<TokenInfoDto> GetTokenInfoAsync(string chainId, string symbol)
    {
        var result = new TokenInfoDto()
        {
            Symbol = symbol,
            Supply = 1,
            TotalSupply = 1,
            TokenName = symbol,
            Issued = 1,
            Decimals = 0,
            IssueChainId = ChainHelper.ConvertBase58ToChainId("AELF"),
            Issuer = Address.FromPublicKey("aa".HexToByteArray()).ToBase58(),
        };
        result.ExternalInfo = new MapField<string, string>
        {
            new Dictionary<string, string>()
            {
                ["__nft_image_url"] = "sss"
            }
        };
        return result;
    }
}