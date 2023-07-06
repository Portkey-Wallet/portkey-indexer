using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Portkey.Contracts.BingoGameContract;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Portkey.Indexer.CA.GraphQL;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class PlayedProcessor : CAHolderTransactionProcessorBase<Played>
{
    private readonly IAElfIndexerClientEntityRepository<BingoGameIndex, TransactionInfo> _bingoIndexRepository;
    public PlayedProcessor(ILogger<PlayedProcessor> logger,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<BingoGameIndex, TransactionInfo> bingoIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderManagerIndex, LogEventInfo> caHolderManagerIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>
            caHolderTransactionIndexRepository,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, LogEventInfo> nftInfoIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo>
            caHolderTransactionAddressIndexRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions, IObjectMapper objectMapper) :
        base(logger, caHolderIndexRepository,caHolderManagerIndexRepository, caHolderTransactionIndexRepository, tokenInfoIndexRepository,
            nftInfoIndexRepository, caHolderTransactionAddressIndexRepository, contractInfoOptions,
            caHolderTransactionInfoOptions, objectMapper)
    {
        _bingoIndexRepository = bingoIndexRepository;
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).BingoGameContractAddress;
    }

    protected override async Task HandleEventAsync(Played eventValue, LogEventContext context)
    {   
        if (eventValue.PlayerAddress == null || eventValue.PlayerAddress.Value == null)
        {
            return;
        }
        // await ProcessCAHolderTransactionAsync(context, eventValue.PlayerAddress.ToBase58());
        if (!IsValidTransaction(context.ChainId, context.To, context.MethodName, context.Params)) return;
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
                Amount = eventValue.Amount / 100000000,
                FromChainId = context.ChainId,
                ToChainId = context.ChainId,
            },
        };
        ObjectMapper.Map(context, transIndex);
        transIndex.MethodName = GetMethodName(context.MethodName, context.Params);

        await CAHolderTransactionIndexRepository.AddOrUpdateAsync(transIndex);
        var index = await _bingoIndexRepository.GetFromBlockStateSetAsync(eventValue.PlayId.ToHex(), context.ChainId);
        if (index != null)
        {
            return;
        }
        var feeMap = GetTransactionFee(context.ExtraProperties);
        List<TransactionFee> feeList;
        if (!feeMap.IsNullOrEmpty())
        {
            feeList = feeMap.Select(pair => new TransactionFee
            {
                Symbol = pair.Key,
                Amount = pair.Value
            }).ToList();
        }
        else
        {
            feeList = new List<TransactionFee>
            {
                new ()
                {
                    Symbol = null,
                    Amount = 0
                }
            };
        }
        // _objectMapper.Map<LogEventContext, CAHolderIndex>(context, caHolderIndex);
        var bingoIndex = new BingoGameIndex
        {
            Id = eventValue.PlayId.ToHex(),
            PlayBlockHeight = eventValue.PlayBlockHeight,
            Amount = eventValue.Amount,
            IsComplete = false,
            PlayId = context.TransactionId,
            BingoType = (int)eventValue.Type,
            Dices = new List<int>{},
            PlayerAddress = eventValue.PlayerAddress.ToBase58(),
            PlayTime = context.BlockTime.ToTimestamp().Seconds,
            PlayTransactionFee = feeList,
            PlayBlockHash = context.BlockHash
        };
        ObjectMapper.Map<LogEventContext, BingoGameIndex>(context, bingoIndex);
        await _bingoIndexRepository.AddOrUpdateAsync(bingoIndex);
    }
}