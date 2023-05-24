using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.BingoGameContract;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class BingoedLogEventProcessor: AElfLogEventProcessorBase<Bingoed,TransactionInfo>
{
    private readonly IObjectMapper _objectMapper;
    private readonly IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> _repository;
    private readonly IAElfIndexerClientEntityRepository<BingoGameIndex, LogEventInfo> _bingoIndexRepository;
    private readonly ContractInfoOptions _contractInfoOptions;
    private readonly IAElfIndexerClientEntityRepository<BingoGameStaticsIndex, LogEventInfo> _bingoStaticsIndexRepository;
    
    public BingoedLogEventProcessor(ILogger<CAHolderCreatedLogEventProcessor> logger, IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<BingoGameIndex, LogEventInfo> bingoIndexRepository,
        IAElfIndexerClientEntityRepository<BingoGameStaticsIndex, LogEventInfo> bingoStaticsIndexRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base((ILogger<AElfLogEventProcessorBase<Bingoed, TransactionInfo>>)logger)
    {
        _objectMapper = objectMapper;
        _repository = repository;
        _bingoIndexRepository = bingoIndexRepository;
        _contractInfoOptions = contractInfoOptions.Value;
        _bingoStaticsIndexRepository = bingoStaticsIndexRepository;
    }
    
    public override string GetContractAddress(string chainId)
    {
        return _contractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).BingoGameContractAddress;
    }

    protected override async Task HandleEventAsync(Bingoed eventValue, LogEventContext context)
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
        if (index == null)
        {
            return;
        }
        // _objectMapper.Map<LogEventContext, CAHolderIndex>(context, caHolderIndex);
        index.BingoBlockHeight = context.BlockHeight;
        index.BingoId = context.TransactionId;
        index.BingoTime = context.BlockTime.Ticks;
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
        index.BingoTransactionFee = feeList;
        index.IsComplete = true;
        index.Dices = eventValue.Dices.Dices.ToList();
        index.Award = eventValue.Award;
        _objectMapper.Map<LogEventContext, BingoGameIndex>(context, index);
        await _bingoIndexRepository.AddOrUpdateAsync(index);
        
        //update bingoStaticsIndex
        var staticsId= IdGenerateHelper.GetId(context.ChainId, eventValue.PlayerAddress.ToBase58());
        var bingoStaticsIndex = await _bingoStaticsIndexRepository.GetFromBlockStateSetAsync(staticsId, context.ChainId);
        if (bingoStaticsIndex == null)
        {
            bingoStaticsIndex = new BingoGameStaticsIndex
            {
                Id = staticsId,
                PlayerAddress = eventValue.PlayerAddress.ToBase58(),
                Amount = eventValue.Amount,
                Award = eventValue.Award,
                TotalWins = eventValue.Award > 0 ? 1 : 0,
                TotalPlays = 1
            };
        }
        else
        {
            bingoStaticsIndex.Amount += eventValue.Amount;
            bingoStaticsIndex.Award += eventValue.Award;
            bingoStaticsIndex.TotalPlays += 1;
            bingoStaticsIndex.TotalWins += eventValue.Award > 0 ? 1 : 0;
        }
        _objectMapper.Map<LogEventContext, BingoGameStaticsIndex>(context, bingoStaticsIndex);
        await _bingoStaticsIndexRepository.AddOrUpdateAsync(bingoStaticsIndex); 
    }
}