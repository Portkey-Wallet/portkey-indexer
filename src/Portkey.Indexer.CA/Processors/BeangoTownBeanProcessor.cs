using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.BeangoTownContract;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class BeangoTownBeanProcessor : CAHolderTransactionProcessorBase<Bingoed>
{
    private readonly IAElfIndexerClientEntityRepository<BeangoTownIndex, TransactionInfo> _bingoIndexRepository;
    private readonly string _methodName = "BeanGoTown-Bingo";
    private readonly ILogger<BeangoTownBeanProcessor> _logger;

    public BeangoTownBeanProcessor(ILogger<BeangoTownBeanProcessor> logger,
        IAElfIndexerClientEntityRepository<BeangoTownIndex, TransactionInfo> bingoIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderManagerIndex, LogEventInfo> caHolderManagerIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>
            caHolderTransactionIndexRepository,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, LogEventInfo> nftInfoIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo>
            caHolderTransactionAddressIndexRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions, IObjectMapper objectMapper) :
        base(logger, caHolderIndexRepository, caHolderManagerIndexRepository, caHolderTransactionIndexRepository,
            tokenInfoIndexRepository,
            nftInfoIndexRepository, caHolderTransactionAddressIndexRepository, contractInfoOptions,
            caHolderTransactionInfoOptions, objectMapper)
    {
        _bingoIndexRepository = bingoIndexRepository;
        _logger = logger;
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).BeangoTownContractAddress;
    }

    protected override async Task HandleEventAsync(Bingoed eventValue, LogEventContext context)
    {
        _logger.LogInformation("recive Bingoed,eventValue.PlayerAddress:{eventValue.PlayerAddress}",
            eventValue.PlayerAddress);
        if (eventValue.PlayerAddress == null || eventValue.PlayerAddress.Value == null)
        {
            return;
        }
        _logger.LogInformation("recive Bingoed,eventValue.IsValidTransaction:{IsValidTransaction}",
            IsValidTransaction(context.ChainId, context.To, context.MethodName, context.Params));
        // await ProcessCAHolderTransactionAsync(context, eventValue.PlayerAddress.ToBase58());
        if (!IsValidTransaction(context.ChainId, context.To, context.MethodName, context.Params)) return;
        var holder = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.PlayerAddress.ToBase58()), context.ChainId);
        _logger.LogInformation("recive Bingoed,holder:{holder},id:{id}",
            holder, IdGenerateHelper.GetId(context.ChainId,
                eventValue.PlayerAddress.ToBase58()));
        if (holder == null) return;

        var transIndex = new CAHolderTransactionIndex
        {
            Id = IdGenerateHelper.GetId(context.BlockHash, context.TransactionId),
            Timestamp = context.BlockTime.ToTimestamp().Seconds,
            FromAddress = eventValue.PlayerAddress.ToBase58(),
            TransactionFee = GetTransactionFee(context.ExtraProperties),
            TransferInfo = new TransferInfo
            {
                FromAddress = GetContractAddress(context.ChainId),
                ToAddress = eventValue.PlayerAddress.ToBase58(),
                //Amount = (eventValue.Amount + eventValue.Award) / 100000000,
                Amount = 0,
                FromChainId = context.ChainId,
                ToChainId = context.ChainId,
            },
        };
        ObjectMapper.Map(context, transIndex);
        transIndex.MethodName = _methodName;
        _logger.LogInformation("recive Bingoed,MethodName:{_methodName}",
            _methodName);
        await CAHolderTransactionIndexRepository.AddOrUpdateAsync(transIndex);
        
        var index = await _bingoIndexRepository.GetFromBlockStateSetAsync(eventValue.PlayId.ToHex(), context.ChainId);
        if (index == null)
        {
            return;
        }
        
        await SaveGameIndexAsync(index, eventValue, context,"");
    }
    
    private async Task SaveGameIndexAsync(BeangoTownIndex townIndex, Bingoed eventValue, LogEventContext context,
        string? seasonId)
    {
        var feeAmount = GetFeeAmount(context.ExtraProperties);
        
        townIndex.SeasonId = seasonId;
        ObjectMapper.Map(eventValue, townIndex);
        townIndex.BingoTransactionInfo = new TransactionInfoIndex()
        {
            TransactionId = context.TransactionId,
            TriggerTime = context.BlockTime,
            TransactionFee = feeAmount
        };
        ObjectMapper.Map(context, townIndex);
        await _bingoIndexRepository.AddOrUpdateAsync(townIndex);
    }
}