using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class ManagerAddedLogEventProcessor: CAHolderManagerProcessorBase<ManagerInfoAdded>
{
    public ManagerAddedLogEventProcessor(ILogger<ManagerAddedLogEventProcessor> logger,
        IObjectMapper objectMapper, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<CAHolderManagerIndex, LogEventInfo> caHolderManagerIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderManagerChangeRecordIndex, LogEventInfo> changeRecordRepository) :
        base(logger, objectMapper, contractInfoOptions, repository,caHolderManagerIndexRepository, changeRecordRepository)
    {
    }
    
    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).CAContractAddress;
    }
    
    protected override async Task HandleEventAsync(ManagerInfoAdded eventValue, LogEventContext context)
    {
        //check manager is already exist in caHolderManagerIndex
        var managerIndexId = IdGenerateHelper.GetId(context.ChainId, eventValue.Manager.ToBase58());
        var caHolderManagerIndex =
            await CAHolderManagerIndexRepository.GetFromBlockStateSetAsync(managerIndexId, context.ChainId);
        if (caHolderManagerIndex == null)
        {
            caHolderManagerIndex = new CAHolderManagerIndex
            {
                Id = managerIndexId,
                Manager = eventValue.Manager.ToBase58(),
                CAAddresses = new List<string>()
                {
                    eventValue.CaAddress.ToBase58()
                }
            };
        }
        else
        {
            if (!caHolderManagerIndex.CAAddresses.Contains(eventValue.CaAddress.ToBase58()))
            {
                caHolderManagerIndex.CAAddresses.Add(eventValue.CaAddress.ToBase58());
            }
        }
        ObjectMapper.Map<LogEventContext, CAHolderManagerIndex>(context, caHolderManagerIndex);
        await CAHolderManagerIndexRepository.AddOrUpdateAsync(caHolderManagerIndex);
        
        //check ca address if already exist in caHolderIndex
        var indexId = IdGenerateHelper.GetId(context.ChainId, eventValue.CaAddress.ToBase58());
        var caHolderIndex = await Repository.GetFromBlockStateSetAsync(indexId, context.ChainId);
        if (caHolderIndex == null)
        {
            return;
        }
        ObjectMapper.Map(context, caHolderIndex);
        if (caHolderIndex.ManagerInfos.Count(m => m.Address == eventValue.Manager.ToBase58()) == 0)
        {
            caHolderIndex.ManagerInfos.Add(new Entities.ManagerInfo
            {
                Address = eventValue.Manager.ToBase58(),
                ExtraData = eventValue.ExtraData
            });
        }

        await Repository.AddOrUpdateAsync(caHolderIndex);

        await AddChangeRecordAsync(eventValue.CaAddress.ToBase58(), eventValue.CaHash.ToHex(),
            eventValue.Manager.ToBase58(), nameof(ManagerInfoAdded), context);
    }
}