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
    private readonly ContractInfoOptions _contractInfoOptions;
    
    public CAHolderCreatedProcessor(ILogger<CAHolderCreatedProcessor> logger, IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> repository, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base(logger)
    {
        _objectMapper = objectMapper;
        _repository = repository;
        _contractInfoOptions = contractInfoOptions.Value;
    }
    
    public override string GetContractAddress(string chainId)
    {
        return _contractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).CAContractAddress;
    }

    protected override async Task HandleEventAsync(CAHolderCreated eventValue, LogEventContext context)
    {
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
            Managers = new List<ManagerInfo>()
            {
                new ManagerInfo()
                {
                    Manager = eventValue.Manager.ToBase58(),
                    DeviceString = eventValue.DeviceString
                }
            }
        };
        _objectMapper.Map<LogEventContext, CAHolderIndex>(context, caHolderIndex);
        await _repository.AddOrUpdateAsync(caHolderIndex);
    }
}