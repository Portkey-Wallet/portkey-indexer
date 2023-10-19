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

public class TokenTransferredLogEventProcessor : CAHolderTokenBalanceProcessorBase<Transferred>
{
    private readonly ILogger<TokenTransferredLogEventProcessor> _logger;

    public TokenTransferredLogEventProcessor(ILogger<TokenTransferredLogEventProcessor> logger,
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

    protected override async Task HandleEventAsync(Transferred eventValue, LogEventContext context)
    {
        var addressTo = eventValue.To.ToBase58();
        var addressFrom = eventValue.From.ToBase58();
        if (!(CheckHelper.CheckNeedRecordBalance(addressTo, SubscribersOptions, eventValue.Symbol) ||
              CheckHelper.CheckNeedRecordBalance(addressFrom, SubscribersOptions, eventValue.Symbol)))
        {
            return;
        }

        var address = addressFrom;
        var amount = -eventValue.Amount;
        if (!CheckHelper.CheckNeedModifyBalance(address, SubscribersOptions))
        {
            address = addressTo;
            amount = eventValue.Amount;
        }

        var recordId = await AddBalanceRecordAsync(address, BalanceChangeType.TokenTransferred, context);
        _logger.LogInformation(
            "In {processor}, caAddress:{address}, symbol:{symbol}, amount:{amount}, transactionId:{transactionId}",
            nameof(TokenTransferredLogEventProcessor), address, eventValue.Symbol, amount,
            context.TransactionId);

        var from = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.From.ToBase58()), context.ChainId);
        if (from != null)
        {
            await ModifyBalanceAsync(from.CAAddress, eventValue.Symbol, -eventValue.Amount, context, recordId);
        }

        var to = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.To.ToBase58()), context.ChainId);
        if (to == null) return;
        await ModifyBalanceAsync(to.CAAddress, eventValue.Symbol, eventValue.Amount, context, recordId);
    }
}