using System.Text.Json;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class ManagerApprovedLogEventProcessor : AElfLogEventProcessorBase<ManagerApproved, LogEventInfo>
{
    private readonly IAElfIndexerClientEntityRepository<ManagerApprovedIndex, LogEventInfo> _repository;
    private readonly ContractInfoOptions _contractInfoOptions;
    private readonly IObjectMapper _objectMapper;
    private readonly ILogger<ManagerApprovedLogEventProcessor> _logger;

    public ManagerApprovedLogEventProcessor(ILogger<ManagerApprovedLogEventProcessor> logger,
        IObjectMapper objectMapper, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<ManagerApprovedIndex, LogEventInfo> repository) : base(logger)
    {
        _logger = logger;
        _objectMapper = objectMapper;
        _repository = repository;
        _contractInfoOptions = contractInfoOptions.Value;
    }

    public override string GetContractAddress(string chainId)
    {
        return _contractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).CAContractAddress;
    }

    protected override async Task HandleEventAsync(ManagerApproved eventValue, LogEventContext context)
    {
        var indexId = IdGenerateHelper.GetId(context.ChainId, context.TransactionId);
        var index = await _repository.GetFromBlockStateSetAsync(indexId, context.ChainId);
        if (index == null)
        {
            index = new ManagerApprovedIndex
            {
                Id = indexId,
                CaHash = eventValue.CaHash.ToHex(),
                Spender = eventValue.Spender.ToBase58(),
                Symbol = eventValue.Symbol,
                Amount = eventValue.Amount,
                External = eventValue.External == null || eventValue.External.Value.Count == 0
                    ? ""
                    : JsonSerializer.Serialize(eventValue.External.Value)
            };
            _objectMapper.Map(context, index);
            await _repository.AddOrUpdateAsync(index);
        }

        _logger.LogDebug("[ManagerApproved]id: {id} transactionId: {transactionId}", indexId, context.TransactionId);
    }
}