using AElf.Contracts.MultiToken;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class TokenCreatedProcessor : AElfLogEventProcessorBase<TokenCreated, TransactionInfo>
{
    private readonly IAElfIndexerClientEntityRepository<TokenInfoIndex, TransactionInfo> _tokenInfoIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<NFTCollectionInfoIndex, TransactionInfo>
        _nftCollectionInfoIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<NFTInfoIndex, TransactionInfo> _nftInfoIndexRepository;
    private readonly ContractInfoOptions _contractInfoOptions;
    private readonly IObjectMapper _objectMapper;

    public TokenCreatedProcessor(ILogger<TokenCreatedProcessor> logger,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, TransactionInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTCollectionInfoIndex, TransactionInfo> nftCollectionInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, TransactionInfo> nftInfoIndexRepository,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base(logger)
    {
        _tokenInfoIndexRepository = tokenInfoIndexRepository;
        _nftCollectionInfoIndexRepository = nftCollectionInfoIndexRepository;
        _nftInfoIndexRepository = nftInfoIndexRepository;
        _objectMapper = objectMapper;
        _contractInfoOptions = contractInfoOptions.Value;
    }

    public override string GetContractAddress(string chainId)
    {
        return _contractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).TokenContractAddress;
    }

    protected override async Task HandleEventAsync(TokenCreated eventValue, LogEventContext context)
    {
        TokenType tokenType = TokenHelper.GetTokenType(eventValue.Symbol);
        if (tokenType == TokenType.Token)
        {
            var id = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol);
            var tokenInfoIndex = await _tokenInfoIndexRepository.GetFromBlockStateSetAsync(id, context.ChainId);
            if (tokenInfoIndex != null) return;
            tokenInfoIndex = new TokenInfoIndex
            {
                Id = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol),
                TokenContractAddress = GetContractAddress(context.ChainId)
            };
            _objectMapper.Map(eventValue, tokenInfoIndex);
            tokenInfoIndex.Type = TokenHelper.GetTokenType(eventValue.Symbol);

            if (eventValue.ExternalInfo is { Value.Count: > 0 })
            {
                tokenInfoIndex.ExternalInfoDictionary = eventValue.ExternalInfo.Value
                    .Where(t => !t.Key.IsNullOrWhiteSpace())
                    .ToDictionary(item => item.Key, item => item.Value);
            }

            tokenInfoIndex.Issuer = eventValue.Issuer.ToBase58();
            _objectMapper.Map(context, tokenInfoIndex);
            tokenInfoIndex.ExternalInfoDictionary ??= new Dictionary<string, string>();

            await _tokenInfoIndexRepository.AddOrUpdateAsync(tokenInfoIndex);
        }


        if (tokenType == TokenType.NFTCollection)
        {
            var id = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol);
            var nftCollectionInfoIndex =
                await _nftCollectionInfoIndexRepository.GetFromBlockStateSetAsync(id, context.ChainId);
            if (nftCollectionInfoIndex != null) return;
            nftCollectionInfoIndex = new NFTCollectionInfoIndex()
            {
                Id = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol),
                TokenContractAddress = GetContractAddress(context.ChainId)
            };
            _objectMapper.Map(eventValue, nftCollectionInfoIndex);
            nftCollectionInfoIndex.Type = TokenHelper.GetTokenType(eventValue.Symbol);
            nftCollectionInfoIndex.Issuer = eventValue.Issuer.ToBase58();
            if (eventValue.ExternalInfo is { Value.Count: > 0 })
            {
                var externalInfo = eventValue.ExternalInfo.Value;
                var buildNftExternalInfo = NftExternalInfoHelper.BuildNftExternalInfo(externalInfo);
                _objectMapper.Map(buildNftExternalInfo, nftCollectionInfoIndex);
            }

            _objectMapper.Map(context, nftCollectionInfoIndex);
            nftCollectionInfoIndex.ExternalInfoDictionary ??= new Dictionary<string, string>();

            await _nftCollectionInfoIndexRepository.AddOrUpdateAsync(nftCollectionInfoIndex);
        }

        if (tokenType == TokenType.NFTItem)
        {
            var id = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol);
            var nftInfoIndex = await _nftInfoIndexRepository.GetFromBlockStateSetAsync(id, context.ChainId);
            if (nftInfoIndex != null) return;
            nftInfoIndex = new NFTInfoIndex()
            {
                Id = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol),
                TokenContractAddress = GetContractAddress(context.ChainId)
            };
            _objectMapper.Map(eventValue, nftInfoIndex);
            nftInfoIndex.Type = TokenHelper.GetTokenType(eventValue.Symbol);
            nftInfoIndex.Issuer = eventValue.Issuer.ToBase58();
            
            if (eventValue.ExternalInfo is { Value.Count: > 0 })
            {
                var externalInfo = eventValue.ExternalInfo.Value;
                var nftExternalInfo = NftExternalInfoHelper.BuildNftExternalInfo(externalInfo);
             
                _objectMapper.Map(nftExternalInfo, nftInfoIndex);
            }

            var nftCollectionSymbol = TokenHelper.GetNFTCollectionSymbol(eventValue.Symbol);
            var collectionId = IdGenerateHelper.GetId(context.ChainId, nftCollectionSymbol);
            var nftCollectionInfoIndex =
                await _nftCollectionInfoIndexRepository.GetFromBlockStateSetAsync(collectionId, context.ChainId);
            if (nftCollectionInfoIndex != null)
            {
                nftInfoIndex.CollectionSymbol = nftCollectionInfoIndex.Symbol;
                nftInfoIndex.CollectionName = nftCollectionInfoIndex.TokenName;
                nftInfoIndex.Lim = nftCollectionInfoIndex.Lim;
                if (!nftCollectionInfoIndex.InscriptionName.IsNullOrWhiteSpace())
                {
                    nftInfoIndex.InscriptionName = nftCollectionInfoIndex.InscriptionName;
                }
            }

            _objectMapper.Map(context, nftInfoIndex);
            nftInfoIndex.ExternalInfoDictionary ??= new Dictionary<string, string>();

            await _nftInfoIndexRepository.AddOrUpdateAsync(nftInfoIndex);
        }
    }
}