using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.BingoGameContract;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;
using Guardian = Portkey.Indexer.CA.Entities.Guardian;

namespace Portkey.Indexer.CA.Processors;

public class BingoedLogEventProcessor: AElfLogEventProcessorBase<Bingoed,LogEventInfo>
{
    private readonly IObjectMapper _objectMapper;
    private readonly IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> _repository;
    private readonly IAElfIndexerClientEntityRepository<BingoIndex, LogEventInfo> _bingoIndexRepository;
    private readonly ContractInfoOptions _contractInfoOptions;
    
    public BingoedLogEventProcessor(ILogger<CAHolderCreatedLogEventProcessor> logger, IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<BingoIndex, LogEventInfo> bingoIndexRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base((ILogger<AElfLogEventProcessorBase<Bingoed, LogEventInfo>>)logger)
    {
        _objectMapper = objectMapper;
        _repository = repository;
        _bingoIndexRepository = bingoIndexRepository;
        _contractInfoOptions = contractInfoOptions.Value;
    }
    
    public override string GetContractAddress(string chainId)
    {
        return _contractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).CAContractAddress;
    }

    protected override async Task HandleEventAsync(Bingoed eventValue, LogEventContext context)
    {   
        //check ca address if already exist in caHolderIndex
        var indexId = IdGenerateHelper.GetId(context.ChainId, eventValue.PlayerAddress.ToBase58());
        var caHolderIndex = await _bingoIndexRepository.GetFromBlockStateSetAsync(indexId, context.ChainId);
        if (caHolderIndex == null)
        {
            return;
        }
        
        // _objectMapper.Map<LogEventContext, CAHolderIndex>(context, caHolderIndex);

        caHolderIndex = new BingoIndex
        {
            Id = indexId,
            play_block_height = context.BlockHeight,
            amount = eventValue.Amount,
            award = eventValue.Award,
            is_complete = eventValue.IsComplete,
            play_id = eventValue.PlayId.ToHex(),
            bingo_id = eventValue.PlayId.ToHex(),
            bingoType = (int)eventValue.Type,
            dices = new List<int>{eventValue.Dices.Dices[0], eventValue.Dices.Dices[1], eventValue.Dices.Dices[2]},
            player_address = eventValue.PlayerAddress.ToBase58(),
        };
        _objectMapper.Map<LogEventContext, BingoIndex>(context, caHolderIndex);
        await _bingoIndexRepository.AddOrUpdateAsync(caHolderIndex);
    }
}