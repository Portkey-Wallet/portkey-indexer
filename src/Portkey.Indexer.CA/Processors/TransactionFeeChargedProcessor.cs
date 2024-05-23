using AElf.Contracts.MultiToken;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Portkey.Indexer.CA.Provider;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class TransactionFeeChargedProcessor : CAHolderTokenBalanceProcessorBase<TransactionFeeCharged>
{
    private readonly IAElfIndexerClientEntityRepository<TransactionFeeChangedIndex, TransactionInfo>
        _transactionFeeChangedIndexRepository;

    private readonly IObjectMapper _objectMapper;

    public TransactionFeeChargedProcessor(ILogger<TransactionFeeChargedProcessor> logger,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<TransactionFeeChangedIndex, TransactionInfo>
            transactionFeeChangedIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, TransactionInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTCollectionInfoIndex, TransactionInfo> nftCollectionInfoRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, TransactionInfo> nftInfoRepository,
        IAElfIndexerClientEntityRepository<CAHolderSearchTokenNFTIndex, TransactionInfo> caHolderSearchTokenNFTRepository,
        IAElfIndexerClientEntityRepository<CAHolderTokenBalanceIndex, TransactionInfo>
            caHolderTokenBalanceIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderNFTCollectionBalanceIndex, TransactionInfo>
            caHolderNFTCollectionBalanceIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderNFTBalanceIndex, TransactionInfo> caHolderNFTBalanceIndexRepository,
        IAElfDataProvider aelfDataProvider,
        IObjectMapper objectMapper,IOptionsSnapshot<InscriptionListOptions> inscriptionListOptions,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo> caHolderTransactionIndexRepository) : base(logger, contractInfoOptions,
        caHolderIndexRepository, tokenInfoIndexRepository, nftCollectionInfoRepository, nftInfoRepository,
        caHolderSearchTokenNFTRepository,
        caHolderTokenBalanceIndexRepository, caHolderNFTCollectionBalanceIndexRepository,
        caHolderNFTBalanceIndexRepository, aelfDataProvider, objectMapper,inscriptionListOptions,
        null, caHolderTransactionIndexRepository)
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

        var indexId = IdGenerateHelper.GetId(context.ChainId, eventValue.ChargingAddress, context.BlockHash);
        var transactionFeeChangedIndex = new TransactionFeeChangedIndex
        {
            Id = indexId,
            ConsumerAddress = eventValue.ChargingAddress.ToBase58(),
        };
        _objectMapper.Map(eventValue, transactionFeeChangedIndex);
        _objectMapper.Map(context, transactionFeeChangedIndex);

        var caHolderIndex = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(
            context.ChainId, eventValue.ChargingAddress.ToBase58()), context.ChainId);
        if (caHolderIndex != null)
        {
            transactionFeeChangedIndex.CAAddress = caHolderIndex.CAAddress;
            await ModifyBalanceAsync(caHolderIndex.CAAddress, eventValue.Symbol, -eventValue.Amount, context);
        }

        await _transactionFeeChangedIndexRepository.AddOrUpdateAsync(transactionFeeChangedIndex);
        await HandlerTransactionIndexAsync(eventValue, context);
    }

    protected override async Task HandlerTransactionIndexAsync(TransactionFeeCharged eventValue, LogEventContext context)
    {
        var id = IdGenerateHelper.GetId(context.BlockHash, context.TransactionId);
        var transIndex = await CAHolderTransactionIndexRepository.GetFromBlockStateSetAsync(id, context.ChainId);
        transIndex ??= new CAHolderTransactionIndex
        {
            Id = id,
            Timestamp = context.BlockTime.ToTimestamp().Seconds,
            FromAddress = context.From,
            ToContractAddress = GetToContractAddress(context.ChainId, context.To, context.MethodName, context.Params)
        };
        if (transIndex.TransactionFee.TryGetValue(eventValue.Symbol, out _))
        {
            transIndex.TransactionFee[eventValue.Symbol] += eventValue.Amount;
        }
        else
        {
            transIndex.TransactionFee[eventValue.Symbol] = eventValue.Amount;
        }

        ObjectMapper.Map(context, transIndex);
        transIndex.MethodName = GetMethodName(context.MethodName, context.Params);
        await CAHolderTransactionIndexRepository.AddOrUpdateAsync(transIndex);
    }
}