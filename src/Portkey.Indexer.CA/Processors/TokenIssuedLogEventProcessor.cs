using AElf.Contracts.MultiToken;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Portkey.Indexer.CA.Options;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class TokenIssuedLogEventProcessor : CAHolderTokenBalanceProcessorBase<Issued>
{
    public TokenIssuedLogEventProcessor(ILogger<TokenIssuedLogEventProcessor> logger,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IOptionsSnapshot<SubscribersOptions> subscribersOptions,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTCollectionInfoIndex, LogEventInfo> nftCollectionInfoRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, LogEventInfo> nftInfoRepository,
        IAElfIndexerClientEntityRepository<CAHolderSearchTokenNFTIndex, LogEventInfo> caHolderSearchTokenNFTRepository,
        IAElfIndexerClientEntityRepository<CAHolderTokenBalanceIndex, LogEventInfo>
            caHolderTokenBalanceIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderNFTCollectionBalanceIndex, LogEventInfo>
            caHolderNFTCollectionBalanceIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderNFTBalanceIndex, LogEventInfo> caHolderNFTBalanceIndexRepository,
        IAElfIndexerClientEntityRepository<BalanceChangeRecordIndex, LogEventInfo> balanceChangeRecordRepository,
        IObjectMapper objectMapper) : base(logger, contractInfoOptions, subscribersOptions,
        caHolderIndexRepository, tokenInfoIndexRepository, nftCollectionInfoRepository, nftInfoRepository,
        caHolderSearchTokenNFTRepository,
        caHolderTokenBalanceIndexRepository, caHolderNFTCollectionBalanceIndexRepository,
        caHolderNFTBalanceIndexRepository, balanceChangeRecordRepository, objectMapper)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).TokenContractAddress;
    }

    protected override async Task HandleEventAsync(Issued eventValue, LogEventContext context)
    {
        var address = eventValue.To.ToBase58();
        if (!CheckHelper.CheckNeedRecordBalance(address, SubscribersOptions, eventValue.Symbol))
        {
            return;
        }

        await AddBalanceRecordAsync(address, BalanceChangeType.TokenIssued, context);
        Logger.LogInformation(
            "In {processor}, caAddress:{address}, symbol:{symbol}, amount:{amount}, transactionId:{transactionId}",
            nameof(TokenIssuedLogEventProcessor), address, eventValue.Symbol, eventValue.Amount,
            context.TransactionId);

        //await UpdateTokenSupply(eventValue, context);

        var holder = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.To.ToBase58()), context.ChainId);
        if (holder == null)
        {
            Logger.LogError(
                "Holder is null, in {processor}, caAddress:{address}, symbol:{symbol}, amount:{amount}, transactionId:{transactionId}",
                nameof(TokenIssuedLogEventProcessor), address, eventValue.Symbol, eventValue.Amount,
                context.TransactionId);
            return;
        }

        await ModifyBalanceAsync(holder.CAAddress, eventValue.Symbol, eventValue.Amount, context);
    }

    private async Task UpdateTokenSupply(Issued eventValue, LogEventContext context)
    {
        TokenType tokenType = TokenHelper.GetTokenType(eventValue.Symbol);

        if (tokenType == TokenType.Token)
        {
            var id = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol);
            var tokenInfoIndex = await TokenInfoIndexRepository.GetFromBlockStateSetAsync(id, context.ChainId);
            if (tokenInfoIndex != null)
            {
                tokenInfoIndex.Supply += eventValue.Amount;
                ObjectMapper.Map(context, tokenInfoIndex);
                await TokenInfoIndexRepository.AddOrUpdateAsync(tokenInfoIndex);
            }
        }

        if (tokenType == TokenType.NFTCollection)
        {
            var id = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol);
            var nftCollectionInfoIndex =
                await NftCollectionInfoRepository.GetFromBlockStateSetAsync(id, context.ChainId);
            if (nftCollectionInfoIndex != null)
            {
                nftCollectionInfoIndex.Supply += eventValue.Amount;
                ObjectMapper.Map(context, nftCollectionInfoIndex);
                await NftCollectionInfoRepository.AddOrUpdateAsync(nftCollectionInfoIndex);
            }
        }

        if (tokenType == TokenType.NFTItem)
        {
            var id = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol);
            var nftInfoIndex = await NftInfoRepository.GetFromBlockStateSetAsync(id, context.ChainId);
            if (nftInfoIndex != null)
            {
                nftInfoIndex.Supply += eventValue.Amount;
                ObjectMapper.Map(context, nftInfoIndex);
                await NftInfoRepository.AddOrUpdateAsync(nftInfoIndex);
            }
        }
    }
}