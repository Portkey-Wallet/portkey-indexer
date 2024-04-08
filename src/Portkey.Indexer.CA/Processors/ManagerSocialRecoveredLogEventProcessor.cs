using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

[Obsolete]
public class ManagerSocialRecoveredLogEventProcessor: AElfLogEventProcessorBase<ManagerInfoSocialRecovered,LogEventInfo>
{
    private readonly IObjectMapper _objectMapper;
    private readonly IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> _repository;
    private readonly IAElfIndexerClientEntityRepository<CAHolderManagerIndex, LogEventInfo> _caHolderManagerIndexRepository;
    private readonly ContractInfoOptions _contractInfoOptions;
    
    public ManagerSocialRecoveredLogEventProcessor(ILogger<ManagerSocialRecoveredLogEventProcessor> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<CAHolderManagerIndex, LogEventInfo> caHolderManagerIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> repository) : base(logger)
    {
        _objectMapper = objectMapper;
        _repository = repository;
        _caHolderManagerIndexRepository = caHolderManagerIndexRepository;
        _contractInfoOptions = contractInfoOptions.Value;
    }
    
    public override string GetContractAddress(string chainId)
    {
        return _contractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).CAContractAddress;
    }
    
    protected override async Task HandleEventAsync(ManagerInfoSocialRecovered eventValue, LogEventContext context)
    {
        //check manager is already exist in caHolderManagerIndex
        var managerIndexId = IdGenerateHelper.GetId(context.ChainId, eventValue.Manager.ToBase58());
        var caHolderManagerIndex =
            await _caHolderManagerIndexRepository.GetFromBlockStateSetAsync(managerIndexId, context.ChainId);
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
        _objectMapper.Map<LogEventContext, CAHolderManagerIndex>(context, caHolderManagerIndex);
        await _caHolderManagerIndexRepository.AddOrUpdateAsync(caHolderManagerIndex);
        
        //check ca address if already exist in caHolderIndex
        var indexId = IdGenerateHelper.GetId(context.ChainId, eventValue.CaAddress.ToBase58());
        var caHolderIndex = await _repository.GetFromBlockStateSetAsync(indexId,context.ChainId);
        if (caHolderIndex == null)
        {
            return;
        }
        _objectMapper.Map<LogEventContext, CAHolderIndex>(context, caHolderIndex);
        if (caHolderIndex.ManagerInfos.Count(m => m.Address == eventValue.Manager.ToBase58()) == 0)
        {
            caHolderIndex.ManagerInfos.Add(new Entities.ManagerInfo()
            {
                Address = eventValue.Manager.ToBase58(),
                ExtraData = eventValue.ExtraData
            });
        }

        await _repository.AddOrUpdateAsync(caHolderIndex);
    }
}