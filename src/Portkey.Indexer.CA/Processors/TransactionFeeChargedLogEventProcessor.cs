using AElf.Contracts.MultiToken;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class TransactionFeeChargedLogEventProcessor : CAHolderTokenBalanceProcessorBase<TransactionFeeCharged>
{
    private readonly IAElfIndexerClientEntityRepository<TransactionFeeChangedIndex, LogEventInfo>
        _transactionFeeChangedIndexRepository;

    private readonly IObjectMapper _objectMapper;

    public TransactionFeeChargedLogEventProcessor(ILogger<TransactionFeeChargedLogEventProcessor> logger,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
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
        IObjectMapper objectMapper) : base(logger, contractInfoOptions,
        caHolderIndexRepository, tokenInfoIndexRepository, nftCollectionInfoRepository, nftInfoRepository,
        caHolderSearchTokenNFTRepository,
        caHolderTokenBalanceIndexRepository, caHolderNFTCollectionBalanceIndexRepository,
        caHolderNFTBalanceIndexRepository, objectMapper)
    {
        _transactionFeeChangedIndexRepository = transactionFeeChangedIndexRepository;
        _objectMapper = objectMapper;
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).TokenContractAddress;
    }

    protected override async Task HandleEventAsync(TransactionFeeCharged eventValue, LogEventContext context)
    {
        if (eventValue.ChargingAddress == null) return;
        var caHolderIndex = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(
            context.ChainId, eventValue.ChargingAddress.ToBase58()), context.ChainId);

        var transactionFeeChangedIndex = new TransactionFeeChangedIndex
        {
            Id = IdGenerateHelper.GetId(context.ChainId, eventValue.ChargingAddress, context.BlockHash),
            ConsumerAddress = eventValue.ChargingAddress.ToBase58(),
            CAAddress = caHolderIndex.CAAddress,
        };
        _objectMapper.Map(eventValue, transactionFeeChangedIndex);
        _objectMapper.Map(context, transactionFeeChangedIndex);
        await _transactionFeeChangedIndexRepository.AddOrUpdateAsync(transactionFeeChangedIndex);

        if (caHolderIndex != null)
        {
            await ModifyBalanceAsync(caHolderIndex.CAAddress, eventValue.Symbol, -eventValue.Amount, context);
        }
    }
}