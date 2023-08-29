using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class TransferLimitChangedLogEventProcessor : AElfLogEventProcessorBase<TransferLimitChanged, LogEventInfo>
{
    private readonly IAElfIndexerClientEntityRepository<TransferLimitIndex, LogEventInfo> _repository;
    private readonly ContractInfoOptions _contractInfoOptions;
    private readonly IObjectMapper _objectMapper;

    public TransferLimitChangedLogEventProcessor(ILogger<TransferLimitChangedLogEventProcessor> logger,
        IObjectMapper objectMapper, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<TransferLimitIndex, LogEventInfo> repository) : base(logger)
    {
        _objectMapper = objectMapper;
        _repository = repository;
        _contractInfoOptions = contractInfoOptions.Value;
    }

    public override string GetContractAddress(string chainId)
    {
        return _contractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).TokenContractAddress;
    }

    protected override async Task HandleEventAsync(TransferLimitChanged eventValue, LogEventContext context)
    {
        var indexId = IdGenerateHelper.GetId(context.ChainId, eventValue.CaHash.ToHex(), eventValue.Symbol);
        var index = await _repository.GetFromBlockStateSetAsync(indexId, context.ChainId);
        if (index == null)
        {
            index = new TransferLimitIndex
            {
                Id = indexId,
                CaHash = eventValue.CaHash.ToHex(),
                Symbol = eventValue.Symbol,
                SingleLimit = eventValue.SingleLimit,
                DailyLimit = eventValue.DailyLimit
            };
            _objectMapper.Map(context, index);
            await _repository.AddOrUpdateAsync(index);
        }
    }
}