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

public class TokenCrossChainReceivedLogEventProcessor : CAHolderTokenBalanceProcessorBase<CrossChainReceived>
{
    private readonly ILogger<TokenCrossChainReceivedLogEventProcessor> _logger;
    public TokenCrossChainReceivedLogEventProcessor(ILogger<TokenCrossChainReceivedLogEventProcessor> logger,
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
        _logger = logger;
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).TokenContractAddress;
    }

    protected override async Task HandleEventAsync(CrossChainReceived eventValue, LogEventContext context)
    {
        var address = eventValue.To.ToBase58();
        if (!CheckHelper.CheckNeedRecordBalance(address, SubscribersOptions, eventValue.Symbol))
        {
            return;
        }

        await AddBalanceRecordAsync(address, BalanceChangeType.TokenCrossChainReceived, context);
        _logger.LogInformation("In {processor}, caAddress:{address}, symbol:{symbol}, amount:{amount}",
            nameof(TokenCrossChainReceivedLogEventProcessor), address, eventValue.Symbol, eventValue.Amount);

        var holder = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.To.ToBase58()), context.ChainId);
        if (holder == null)
        {
            _logger.LogError("Holder is null, in {processor}, caAddress:{address}, symbol:{symbol}, amount:{amount}",
                nameof(TokenCrossChainReceivedLogEventProcessor), address, eventValue.Symbol, eventValue.Amount);
            return;
        }

        await ModifyBalanceAsync(holder.CAAddress, eventValue.Symbol, eventValue.Amount, context);
    }
}