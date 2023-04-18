using AElf.Standards.ACS0;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class ContractDeployedProcessor : AElfLogEventProcessorBase<ContractDeployed,LogEventInfo>
{
    private readonly IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> _tokenInfoIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<NFTProtocolInfoIndex, LogEventInfo> _nftProtocolInfoIndexRepository;
    private readonly InitialInfoOptions _initialInfoOptions;
    private readonly ContractInfoOptions _contractInfoOptions;
    private readonly IObjectMapper _objectMapper;

    public ContractDeployedProcessor(ILogger<ContractDeployedProcessor> logger,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTProtocolInfoIndex, LogEventInfo> nftProtocolInfoIndexRepository,
        IOptionsSnapshot<InitialInfoOptions> initialInfoOptions, IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) :
        base(logger)
    {
        _tokenInfoIndexRepository = tokenInfoIndexRepository;
        _nftProtocolInfoIndexRepository = nftProtocolInfoIndexRepository;
        _objectMapper = objectMapper;
        _contractInfoOptions = contractInfoOptions.Value;
        _initialInfoOptions = initialInfoOptions.Value;
    }

    public override string GetContractAddress(string chainId)
    {
        return _contractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).GenesisContractAddress;
    }

    protected override async Task HandleEventAsync(ContractDeployed eventValue, LogEventContext context)
    {
        if (eventValue.Address.ToBase58() != _contractInfoOptions.ContractInfos.First(c=>c.ChainId == context.ChainId).CAContractAddress) return;
        var nftProtocolInfoList = _initialInfoOptions.NFTProtocolInfoList.Where(n => n.ChainId == context.ChainId).ToList();
        foreach (var nftProtocolInfo in nftProtocolInfoList)
        {
            var nftProtocolInfoIndex = _objectMapper.Map<NFTProtocolInfo, NFTProtocolInfoIndex>(nftProtocolInfo);
            nftProtocolInfoIndex.Id = IdGenerateHelper.GetId(nftProtocolInfo.ChainId, nftProtocolInfo.Symbol);
            nftProtocolInfoIndex.BlockHash = context.BlockHash;
            nftProtocolInfoIndex.BlockHeight = context.BlockHeight;
            nftProtocolInfoIndex.PreviousBlockHash = context.PreviousBlockHash;
            await _nftProtocolInfoIndexRepository.AddOrUpdateAsync(nftProtocolInfoIndex);
        }

        var tokenInfoList = _initialInfoOptions.TokenInfoList.Where(n => n.ChainId == context.ChainId).ToList();
        foreach (var tokenInfo in tokenInfoList)
        {
            var tokenInfoIndex = _objectMapper.Map<TokenInfo, TokenInfoIndex>(tokenInfo);
            tokenInfoIndex.Id = IdGenerateHelper.GetId(tokenInfo.ChainId, tokenInfo.Symbol);
            tokenInfoIndex.BlockHash = context.BlockHash;
            tokenInfoIndex.BlockHeight = context.BlockHeight;
            tokenInfoIndex.PreviousBlockHash = context.PreviousBlockHash;
            await _tokenInfoIndexRepository.AddOrUpdateAsync(tokenInfoIndex);
        }
    }
}