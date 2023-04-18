using AElf.Contracts.MultiToken;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class TokenCreatedProcessor : AElfLogEventProcessorBase<TokenCreated,LogEventInfo>
{
    private readonly IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> _repository;
    private readonly ContractInfoOptions _contractInfoOptions;
    private readonly IObjectMapper _objectMapper;

    public TokenCreatedProcessor(ILogger<TokenCreatedProcessor> logger,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> repository, IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base(logger)
    {
        _repository = repository;
        _objectMapper = objectMapper;
        _contractInfoOptions = contractInfoOptions.Value;
    }

    public override string GetContractAddress(string chainId)
    {
        return _contractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).TokenContractAddress;
    }

    protected override async Task HandleEventAsync(TokenCreated eventValue, LogEventContext context)
    {
        var id = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol);
        var tokenInfoIndex = await _repository.GetFromBlockStateSetAsync(id,context.ChainId);
        if (tokenInfoIndex != null) return;
        tokenInfoIndex = new TokenInfoIndex
        {
            Id = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol),
            TokenContractAddress = GetContractAddress(context.ChainId)
        };
        _objectMapper.Map(eventValue, tokenInfoIndex);
        tokenInfoIndex.Issuer = eventValue.Issuer.ToBase58();
        _objectMapper.Map(context, tokenInfoIndex);
        await _repository.AddOrUpdateAsync(tokenInfoIndex);
    }
}