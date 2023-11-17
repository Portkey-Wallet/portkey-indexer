using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Portkey.Contracts;
using Portkey.Contracts.BeangoTownContract;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Portkey.Indexer.CA.GraphQL;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class BeangoTownGoProcessor : CAHolderTransactionProcessorBase<Played>
{
    private readonly IAElfIndexerClientEntityRepository<BeangoTownIndex, TransactionInfo> _bingoIndexRepository;
    private readonly string _methodName = "BeanGoTown-Play";
    private readonly string _methodNameOrigin = "Play";
    private readonly ILogger<BeangoTownGoProcessor> _logger;
    
    public BeangoTownGoProcessor(ILogger<BeangoTownGoProcessor> logger,
        IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<BeangoTownIndex, TransactionInfo> bingoIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderManagerIndex, TransactionInfo> caHolderManagerIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>
            caHolderTransactionIndexRepository,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, TransactionInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, TransactionInfo> nftInfoIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo>
            caHolderTransactionAddressIndexRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions, IObjectMapper objectMapper) :
        base(logger, caHolderIndexRepository,caHolderManagerIndexRepository, caHolderTransactionIndexRepository, tokenInfoIndexRepository,
            nftInfoIndexRepository, caHolderTransactionAddressIndexRepository, contractInfoOptions,
            caHolderTransactionInfoOptions, objectMapper)
    {
        _bingoIndexRepository = bingoIndexRepository;
        _logger = logger;
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).BeangoTownContractAddress;
    }

    protected override async Task HandleEventAsync(Played eventValue, LogEventContext context)
    {
        if (eventValue.PlayerAddress == null || eventValue.PlayerAddress.Value == null)
        {
            return;
        }
        
        // await ProcessCAHolderTransactionAsync(context, eventValue.PlayerAddress.ToBase58());
        //if (!IsValidTransaction(context.ChainId, context.To, context.MethodName, context.Params)) return;
        var holder = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.PlayerAddress.ToBase58()), context.ChainId);
        if (holder == null) return;

        var transIndex = new CAHolderTransactionIndex
        {
            Id = IdGenerateHelper.GetId(context.BlockHash, context.TransactionId),
            Timestamp = context.BlockTime.ToTimestamp().Seconds,
            FromAddress = eventValue.PlayerAddress.ToBase58(),
            TransactionFee = GetTransactionFee(context.ExtraProperties),
            TransferInfo = new TransferInfo
            {
                FromAddress = eventValue.PlayerAddress.ToBase58(),
                ToAddress = GetContractAddress(context.ChainId),
                Amount = 0,
                FromChainId = context.ChainId,
                ToChainId = context.ChainId,
            },
        };
        ObjectMapper.Map(context, transIndex);
        transIndex.MethodName = GetMethodName(context.MethodName, context.Params);
        if (transIndex.MethodName == _methodNameOrigin)
        {
            transIndex.MethodName = _methodName;
        }

        await CAHolderTransactionIndexRepository.AddOrUpdateAsync(transIndex);
        
        var index = await _bingoIndexRepository.GetFromBlockStateSetAsync(eventValue.PlayId.ToHex(), context.ChainId);
        if (index != null)
        {
            return;
        }
        
        var feeAmount = GetFeeAmount(context.ExtraProperties);
        var townIndex = new BeangoTownIndex
        {
            Id = eventValue.PlayId.ToHex(),
            CaAddress = ToFullAddress(eventValue.PlayerAddress.ToBase58(), context.ChainId),
            PlayBlockHeight = eventValue.PlayBlockHeight,
            PlayTransactionInfo = new TransactionInfoIndex
            {
                TransactionId = context.TransactionId,
                TriggerTime = context.BlockTime,
                TransactionFee = feeAmount
            }
        };
        ObjectMapper.Map(context, townIndex);
        await _bingoIndexRepository.AddOrUpdateAsync(townIndex);
    }
}