using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class TransferSecurityThresholdChangedProcessor : AElfLogEventProcessorBase<
    TransferSecurityThresholdChanged, TransactionInfo>
{
    private readonly IAElfIndexerClientEntityRepository<TransferSecurityThresholdIndex, TransactionInfo> _repository;
    private readonly ContractInfoOptions _contractInfoOptions;
    private readonly IObjectMapper _objectMapper;
    private readonly ILogger<TransferSecurityThresholdChangedProcessor> _processorLogger;

    public TransferSecurityThresholdChangedProcessor(
        ILogger<TransferSecurityThresholdChangedProcessor> logger,
        IObjectMapper objectMapper, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<TransferSecurityThresholdIndex, TransactionInfo> repository) : base(logger)
    {
        _objectMapper = objectMapper;
        _repository = repository;
        _processorLogger = logger;
        _contractInfoOptions = contractInfoOptions.Value;
    }

    public override string GetContractAddress(string chainId)
    {
        return _contractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).CAContractAddress;
    }

    protected override async Task HandleEventAsync(TransferSecurityThresholdChanged eventValue, LogEventContext context)
    {
        var indexId =
            IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol, nameof(TransferSecurityThresholdChanged));
        var  index = new TransferSecurityThresholdIndex
        {
            Id = indexId,
            Symbol = eventValue.Symbol,
            BalanceThreshold = eventValue.BalanceThreshold,
            GuardianThreshold = eventValue.GuardianThreshold
        };
        _objectMapper.Map(context, index);
        await _repository.AddOrUpdateAsync(index);

        _processorLogger.LogDebug("[TransferLimitChanged] id: {indexId} Symbol:{Symbol}", index.Id, index.Symbol);
    }
}