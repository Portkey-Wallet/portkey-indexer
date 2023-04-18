using AElf.Contracts.NFT;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class NFTProtocolCreatedProcessor : AElfLogEventProcessorBase<NFTProtocolCreated,LogEventInfo>
{
    private readonly IAElfIndexerClientEntityRepository<NFTProtocolInfoIndex, LogEventInfo> _repository;
    private readonly ContractInfoOptions _contractInfoOptions;
    private readonly IObjectMapper _objectMapper;

    public NFTProtocolCreatedProcessor(ILogger<NFTProtocolCreatedProcessor> logger, IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<NFTProtocolInfoIndex, LogEventInfo> repository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base(logger)
    {
        _objectMapper = objectMapper;
        _repository = repository;
        _contractInfoOptions = contractInfoOptions.Value;
    }

    public override string GetContractAddress(string chainId)
    {
        return _contractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).NFTContractAddress;
    }

    protected override async Task HandleEventAsync(NFTProtocolCreated eventValue, LogEventContext context)
    {
        var id = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol);
        var nftProtocolInfoIndex = await _repository.GetFromBlockStateSetAsync(id,context.ChainId);
        if (nftProtocolInfoIndex != null) return;
        nftProtocolInfoIndex = _objectMapper.Map<NFTProtocolCreated, NFTProtocolInfoIndex>(eventValue);
        nftProtocolInfoIndex.Id = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol);
        nftProtocolInfoIndex.ImageUrl = 
            eventValue.Metadata.Value?.TryGetValue("ImageUrl", out var imageUrl)??false ? imageUrl : null;
        nftProtocolInfoIndex.Creator = eventValue.Creator.ToBase58();
        nftProtocolInfoIndex.Supply = 0;
        _objectMapper.Map(context, nftProtocolInfoIndex);
        await _repository.AddOrUpdateAsync(nftProtocolInfoIndex);
    }
}