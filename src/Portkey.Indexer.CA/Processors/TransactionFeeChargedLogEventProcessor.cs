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

public class TransactionFeeChargedLogEventProcessor : CAHolderTokenBalanceProcessorBase<TransactionFeeCharged>
{
    private readonly IAElfIndexerClientEntityRepository<TransactionFeeChangedIndex, LogEventInfo>
        _transactionFeeChangedIndexRepository;

    private readonly IObjectMapper _objectMapper;
    private readonly ILogger<TransactionFeeChargedLogEventProcessor> _logger;

    public TransactionFeeChargedLogEventProcessor(ILogger<TransactionFeeChargedLogEventProcessor> logger,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IOptionsSnapshot<SubscribersOptions> subscribersOptions,
        IAElfIndexerClientEntityRepository<TransactionFeeChangedIndex, LogEventInfo>
            transactionFeeChangedIndexRepository,
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
        _transactionFeeChangedIndexRepository = transactionFeeChangedIndexRepository;
        _objectMapper = objectMapper;
        _logger = logger;
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).TokenContractAddress;
    }

    protected override async Task HandleEventAsync(TransactionFeeCharged eventValue, LogEventContext context)
    {
        if (eventValue.ChargingAddress == null)
        {
            _logger.LogError("chargingAddress is null, transactionId:{transactionId}", context.TransactionId);
            return;
        }

        var address = eventValue.ChargingAddress.ToBase58();
        if (!CheckHelper.CheckNeedRecordBalance(address, SubscribersOptions, eventValue.Symbol))
        {
            return;
        }

        var indexId = IdGenerateHelper.GetId(context.ChainId, eventValue.ChargingAddress, context.BlockHash);
        var transactionFeeChangedIndex = new TransactionFeeChangedIndex
        {
            Id = indexId,
            ConsumerAddress = eventValue.ChargingAddress.ToBase58(),
        };
        _objectMapper.Map(eventValue, transactionFeeChangedIndex);
        _objectMapper.Map(context, transactionFeeChangedIndex);

        await AddBalanceRecordAsync(address,BalanceChangeType.TransactionFeeCharged, context);
        _logger.LogInformation(
            "In {processor}, caAddress:{address}, symbol:{symbol}, amount:{amount}, transactionId:{transactionId}",
            nameof(TransactionFeeChargedLogEventProcessor), address, eventValue.Symbol, -eventValue.Amount,
            context.TransactionId);

        var caHolderIndex = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(
            context.ChainId, eventValue.ChargingAddress.ToBase58()), context.ChainId);

        if (caHolderIndex == null)
        {
            _logger.LogError(
                "Holder is null, in {processor}, caAddress:{address}, symbol:{symbol}, amount:{amount}, transactionId:{transactionId}",
                nameof(TokenBurnedLogEventProcessor), address, eventValue.Symbol, -eventValue.Amount,
                context.TransactionId);
        }

        if (caHolderIndex != null)
        {
            transactionFeeChangedIndex.CAAddress = caHolderIndex.CAAddress;
            await ModifyBalanceAsync(caHolderIndex.CAAddress, eventValue.Symbol, -eventValue.Amount, context);
        }

        await _transactionFeeChangedIndexRepository.AddOrUpdateAsync(transactionFeeChangedIndex);
    }
}