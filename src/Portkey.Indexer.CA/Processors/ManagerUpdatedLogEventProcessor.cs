using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class ManagerUpdatedLogEventProcessor : CAHolderManagerProcessorBase<ManagerInfoUpdated>
{
    public ManagerUpdatedLogEventProcessor(ILogger<ManagerUpdatedLogEventProcessor> logger,
        IObjectMapper objectMapper, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<CAHolderManagerIndex, LogEventInfo> caHolderManagerIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderManagerChangeRecordIndex, LogEventInfo> changeRecordRepository) :
        base(logger, objectMapper, contractInfoOptions, repository, caHolderManagerIndexRepository,
            changeRecordRepository)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).CAContractAddress;
    }

    protected override async Task HandleEventAsync(ManagerInfoUpdated eventValue, LogEventContext context)
    {
        //check ca address if already exist in caHolderIndex
        var indexId = IdGenerateHelper.GetId(context.ChainId, eventValue.CaAddress.ToBase58());
        var caHolderIndex = await Repository.GetFromBlockStateSetAsync(indexId, context.ChainId);
        if (caHolderIndex == null) return;

        ObjectMapper.Map(context, caHolderIndex);

        var managerInfo = caHolderIndex.ManagerInfos.FirstOrDefault(m => m.Address == eventValue.Manager.ToBase58());
        if (managerInfo == null) return;

        managerInfo.ExtraData = eventValue.ExtraData;

        await Repository.AddOrUpdateAsync(caHolderIndex);
        await AddChangeRecordAsync(eventValue.CaAddress.ToBase58(), eventValue.CaHash.ToHex(),
            eventValue.Manager.ToBase58(), nameof(ManagerInfoUpdated), context);
    }
}