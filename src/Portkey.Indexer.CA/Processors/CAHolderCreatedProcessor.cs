using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class CAHolderCreatedProcessor: AElfLogEventProcessorBase<CAHolderCreated,LogEventInfo>
{
    private readonly IObjectMapper _objectMapper;
    private readonly IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> _repository;
    private readonly IAElfIndexerClientEntityRepository<CAHolderManagerIndex, LogEventInfo> _caHolderManagerIndexRepository;
    private readonly ContractInfoOptions _contractInfoOptions;
    
    public CAHolderCreatedProcessor(ILogger<CAHolderCreatedProcessor> logger, IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<CAHolderManagerIndex, LogEventInfo> caHolderManagerIndexRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base(logger)
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

    protected override async Task HandleEventAsync(CAHolderCreated eventValue, LogEventContext context)
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
        var caHolderIndex = await _repository.GetFromBlockStateSetAsync(indexId, context.ChainId);
        if (caHolderIndex != null)
        {
            return;
        }
        
        // _objectMapper.Map<LogEventContext, CAHolderIndex>(context, caHolderIndex);

        caHolderIndex = new CAHolderIndex
        {
            Id = indexId,
            CAHash = eventValue.CaHash.ToHex(),
            CAAddress = eventValue.CaAddress.ToBase58(),
            Creator = eventValue.Creator.ToBase58(),
            ManagerInfos = new List<Entities.ManagerInfo>()
            {
                new Entities.ManagerInfo()
                {
                    Address = eventValue.Manager.ToBase58(),
                    ExtraData = eventValue.ExtraData
                }
            }
        };
        _objectMapper.Map<LogEventContext, CAHolderIndex>(context, caHolderIndex);
        await _repository.AddOrUpdateAsync(caHolderIndex);
    }
}