using AElf;
using AElf.Client.Dto;
using AElf.Contracts.MultiToken;
using AElfIndexer.Client.Providers;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Provider;

public interface IAElfDataProvider
{
    Task<TokenInfoDto> GetTokenInfoAsync(string chainId, string symbol);
}

public class AElfDataProvider : IAElfDataProvider
{
    private const string PrivateKey = "09da44778f8db2e602fb484334f37df19e221c84c4582ce5b7770ccfbc3ddbef";
    private readonly IAElfClientProvider _aelfClientProvider;
    private readonly ContractInfoOptions _contractInfoOptions;

    public AElfDataProvider(IAElfClientProvider aElfClientProvider, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions)
    {
        _aelfClientProvider = aElfClientProvider;
        _contractInfoOptions = contractInfoOptions.Value;
    }

    public async Task<TokenInfoDto> GetTokenInfoAsync(string chainId, string symbol)
    {
        var client = _aelfClientProvider.GetClient(chainId);
        var tokenContractAddress = _contractInfoOptions.ContractInfos.First(t => t.ChainId == chainId).TokenContractAddress;
        var transactionGetToken =
            await client.GenerateTransactionAsync(client.GetAddressFromPrivateKey(PrivateKey), tokenContractAddress,
                "GetTokenInfo",
                new GetTokenInfoInput
                {
                    Symbol = symbol
                });
        var txWithSignGetToken = client.SignTransaction(PrivateKey, transactionGetToken);
        var transactionGetTokenResult = await client.ExecuteTransactionAsync(new ExecuteTransactionDto
        {
            RawTransaction = txWithSignGetToken.ToByteArray().ToHex()
        });
        var tokenInfo = AElf.Contracts.MultiToken.TokenInfo.Parser.ParseFrom(
            ByteArrayHelper.HexStringToByteArray(transactionGetTokenResult));
        var tokenInfoDto = new TokenInfoDto
        {
            // not support mapping
            Symbol = tokenInfo.Symbol,
            TokenName = tokenInfo.TokenName,
            Supply = tokenInfo.Supply,
            TotalSupply = tokenInfo.TotalSupply,
            Decimals = tokenInfo.Decimals,
            Issued = tokenInfo.Issued,
            IsBurnable = tokenInfo.IsBurnable,
            IssueChainId = tokenInfo.IssueChainId,
            Issuer = tokenInfo.Issuer.ToBase58(),
            ExternalInfo = tokenInfo.ExternalInfo.Value
        };
        return tokenInfoDto;
    }
}