using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.BingoGameContract;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class PlayedLogEventProcessor: AElfLogEventProcessorBase<Played,TransactionInfo>
{
    private readonly IObjectMapper _objectMapper;
    private readonly IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> _repository;
    private readonly IAElfIndexerClientEntityRepository<BingoGameIndex, LogEventInfo> _bingoIndexRepository;
    private readonly ContractInfoOptions _contractInfoOptions;
    
    public PlayedLogEventProcessor(ILogger<CAHolderCreatedLogEventProcessor> logger, IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<BingoGameIndex, LogEventInfo> bingoIndexRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base((ILogger<AElfLogEventProcessorBase<Played, TransactionInfo>>)logger)
    {
        _objectMapper = objectMapper;
        _repository = repository;
        _bingoIndexRepository = bingoIndexRepository;
        _contractInfoOptions = contractInfoOptions.Value;
    }
    
    public override string GetContractAddress(string chainId)
    {
        return _contractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).BingoGameContractAddress;
    }

    protected override async Task HandleEventAsync(Played eventValue, LogEventContext context)
    {   
        //check ca address if already exist in caHolderIndex
        if (eventValue.PlayerAddress == null || eventValue.PlayerAddress.Value == null)
        {
            return;
        }
        var indexId = IdGenerateHelper.GetId(context.ChainId, eventValue.PlayerAddress.ToBase58());
        var caHolderIndex = await _repository.GetFromBlockStateSetAsync(indexId, context.ChainId);
        if (caHolderIndex == null)
        {
            return;
        }
        var index = await _bingoIndexRepository.GetFromBlockStateSetAsync(eventValue.PlayId.ToHex(), context.ChainId);
        if (index != null)
        {
            return;
        }
        var feeMap = TransactionFeeHelper.GetTransactionFee(context.ExtraProperties);
        if (feeMap.IsNullOrEmpty())
        {
            return;
        }
        var feeList = feeMap.Select(pair => new TransactionFee
        {
            Symbol = pair.Key,
            Amount = pair.Value
        }).ToList();
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
            PlayTime = context.BlockTime.Ticks,
            PlayTransactionFee = feeList,
        };
        _objectMapper.Map<LogEventContext, BingoGameIndex>(context, bingoIndex);
        await _bingoIndexRepository.AddOrUpdateAsync(bingoIndex);
    }
}