using AElfIndexer.Client;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Handlers;

public class TokenAndNftInitService : IHostedService
{
    private readonly IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> _tokenInfoIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<NFTCollectionInfoIndex, LogEventInfo>
        _nftProtocolInfoIndexRepository;

    private readonly InitialInfoOptions _initialInfoOptions;
    private readonly IObjectMapper _objectMapper;
    private readonly ILogger<TokenAndNftInitService> _logger;

    public TokenAndNftInitService(ILogger<TokenAndNftInitService> logger,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTCollectionInfoIndex, LogEventInfo> nftProtocolInfoIndexRepository,
        IOptionsSnapshot<InitialInfoOptions> initialInfoOptions, IObjectMapper objectMapper)
    {
        _tokenInfoIndexRepository = tokenInfoIndexRepository;
        _nftProtocolInfoIndexRepository = nftProtocolInfoIndexRepository;
        _objectMapper = objectMapper;
        _logger = logger;
        _initialInfoOptions = initialInfoOptions.Value;
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var nftProtocolInfo in _initialInfoOptions.NFTProtocolInfoList)
        {
            var nftProtocolInfoIndex = _objectMapper.Map<NFTProtocolInfo, NFTCollectionInfoIndex>(nftProtocolInfo);
            nftProtocolInfoIndex.Id = IdGenerateHelper.GetId(nftProtocolInfo.ChainId, nftProtocolInfo.Symbol);
            nftProtocolInfoIndex.BlockHash = nftProtocolInfo.BlockHash;
            nftProtocolInfoIndex.BlockHeight = nftProtocolInfo.BlockHeight;
            nftProtocolInfoIndex.PreviousBlockHash = nftProtocolInfo.PreviousBlockHash;
            await _nftProtocolInfoIndexRepository.AddOrUpdateAsync(nftProtocolInfoIndex);
            _logger.LogInformation("add nft success, tokenName:{tokenName}, chainId:{chainId}", nftProtocolInfo.Symbol,
                nftProtocolInfo.ChainId);
        }

        foreach (var tokenInfo in _initialInfoOptions.TokenInfoList)
        {
            var tokenInfoIndex = _objectMapper.Map<TokenInfo, TokenInfoIndex>(tokenInfo);
            tokenInfoIndex.Id = IdGenerateHelper.GetId(tokenInfo.ChainId, tokenInfo.Symbol);
            tokenInfoIndex.BlockHash = tokenInfo.BlockHash;
            tokenInfoIndex.BlockHeight = tokenInfo.BlockHeight;
            tokenInfoIndex.PreviousBlockHash = tokenInfo.PreviousBlockHash;
            await _tokenInfoIndexRepository.AddOrUpdateAsync(tokenInfoIndex);
            _logger.LogInformation("add token success, tokenName:{tokenName}, chainId:{chainId}", tokenInfo.Symbol,
                tokenInfo.ChainId);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}