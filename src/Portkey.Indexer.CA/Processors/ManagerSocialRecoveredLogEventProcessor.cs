using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class ManagerSocialRecoveredLogEventProcessor: AElfLogEventProcessorBase<ManagerSocialRecovered,LogEventInfo>
{
    private readonly IObjectMapper _objectMapper;
    private readonly IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> _repository;
    private readonly ContractInfoOptions _contractInfoOptions;
    
    public ManagerSocialRecoveredLogEventProcessor(ILogger<ManagerSocialRecoveredLogEventProcessor> logger,IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> repository) : base(logger)
    {
        _objectMapper = objectMapper;
        _repository = repository;
        _contractInfoOptions = contractInfoOptions.Value;
    }
    
    public override string GetContractAddress(string chainId)
    {
        return _contractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).CAContractAddress;
    }
    
    protected override async Task HandleEventAsync(ManagerSocialRecovered eventValue, LogEventContext context)
    {
        var indexId = IdGenerateHelper.GetId(context.ChainId, eventValue.CaAddress.ToBase58());
        var caHolderIndex = await _repository.GetFromBlockStateSetAsync(indexId,context.ChainId);
        if (caHolderIndex == null)
        {
            return;
        }
        _objectMapper.Map<LogEventContext, CAHolderIndex>(context, caHolderIndex);
        if (caHolderIndex.Managers.Count(m => m.Manager == eventValue.Manager.ToBase58()) == 0)
        {
            caHolderIndex.Managers.Add(new ManagerInfo()
            {
                Manager = eventValue.Manager.ToBase58(),
                DeviceString = eventValue.DeviceString
            });
        }

        await _repository.AddOrUpdateAsync(caHolderIndex);
    }
}