using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class ManagerAddedLogEventProcessor: CAHolderManagerProcessorBase<ManagerAdded>
{
    public ManagerAddedLogEventProcessor(ILogger<ManagerAddedLogEventProcessor> logger,
        IObjectMapper objectMapper, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<CAHolderManagerChangeRecordIndex, LogEventInfo> changeRecordRepository) :
        base(logger, objectMapper, contractInfoOptions, repository, changeRecordRepository)
    {
    }
    
    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).CAContractAddress;
    }
    
    protected override async Task HandleEventAsync(ManagerAdded eventValue, LogEventContext context)
    {
        var indexId = IdGenerateHelper.GetId(context.ChainId, eventValue.CaAddress.ToBase58());
        var caHolderIndex = await Repository.GetFromBlockStateSetAsync(indexId, context.ChainId);
        if (caHolderIndex == null)
        {
            return;
        }
        ObjectMapper.Map(context, caHolderIndex);
        if (caHolderIndex.Managers.Count(m => m.Manager == eventValue.Manager.ToBase58()) == 0)
        {
            caHolderIndex.Managers.Add(new ManagerInfo
            {
                Manager = eventValue.Manager.ToBase58(),
                DeviceString = eventValue.DeviceString
            });
        }

        await Repository.AddOrUpdateAsync(caHolderIndex);

        await AddChangeRecordAsync(eventValue.CaAddress.ToBase58(), eventValue.CaHash.ToHex(),
            eventValue.Manager.ToBase58(), nameof(ManagerAdded), context);
    }
}