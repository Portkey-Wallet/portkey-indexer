using AElf.Contracts.MultiToken;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Portkey.Indexer.CA.Entities;
using Serilog;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class TokenCreatedProcessor : AElfLogEventProcessorBase<TokenCreated, LogEventInfo>
{
    private readonly IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> _tokenInfoIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<NFTCollectionInfoIndex, LogEventInfo>
        _nftCollectionInfoIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<NFTInfoIndex, LogEventInfo> _nftInfoIndexRepository;
    private readonly ContractInfoOptions _contractInfoOptions;
    private readonly IObjectMapper _objectMapper;

    public TokenCreatedProcessor(ILogger<TokenCreatedProcessor> logger,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTCollectionInfoIndex, LogEventInfo> nftCollectionInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, LogEventInfo> nftInfoIndexRepository,
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

        // tokenInfoIndex.TokenExternalInfo = new TokenExternalInfo();
        // if (eventValue.ExternalInfo.Value.ContainsKey("__nft_image_url"))
        // {
        //     tokenInfoIndex.TokenExternalInfo.ImageUrl = eventValue.ExternalInfo.Value["__nft_image_url"];
        // }
        // if (eventValue.ExternalInfo.Value.ContainsKey("__nft_last_item_id"))
        // {
        //     long.TryParse(eventValue.ExternalInfo.Value["__nft_last_item_id"], out long lastItemId);
        //     tokenInfoIndex.TokenExternalInfo.LastItemId = lastItemId;
        // }
        // if (eventValue.ExternalInfo.Value.ContainsKey("__nft_base_uri"))
        // {
        //     tokenInfoIndex.TokenExternalInfo.BaseUrl = eventValue.ExternalInfo.Value["__nft_base_uri"];
        // }
        // if (eventValue.ExternalInfo.Value.ContainsKey("__nft_type"))
        // {
        //     tokenInfoIndex.TokenExternalInfo.Type = eventValue.ExternalInfo.Value["__nft_type"];
        // }
        // if (eventValue.ExternalInfo.Value.ContainsKey("__nft_is_item_id_reuse"))
        // {
        //     bool.TryParse(eventValue.ExternalInfo.Value["__nft_is_item_id_reuse"], out bool isItemIdReuse);
        //     tokenInfoIndex.TokenExternalInfo.IsItemIdReuse = isItemIdReuse;
        // }
        // if (eventValue.ExternalInfo.Value.ContainsKey("__nft_is_burned"))
        // {
        //     bool.TryParse(eventValue.ExternalInfo.Value["__nft_is_burned"], out bool isBurned);
        //     tokenInfoIndex.TokenExternalInfo.IsBurned = isBurned;
        // }

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
                //
                //
                // nftCollectionInfoIndex.ExternalInfoDictionary = eventValue.ExternalInfo.Value
                //     .Where(t => !t.Key.IsNullOrWhiteSpace())
                //     .ToDictionary(item => item.Key, item => item.Value);
                //
                //
                // if (eventValue.ExternalInfo.Value.TryGetValue("__nft_image_url", out var imageUrl))
                // {
                //     nftCollectionInfoIndex.ImageUrl = imageUrl;
                // }
                // else if (eventValue.ExternalInfo.Value.TryGetValue("inscription_image", out var inscriptionImage))
                // {
                //     nftCollectionInfoIndex.ImageUrl = inscriptionImage;
                // }
                // else if(eventValue.ExternalInfo.Value.TryGetValue("__inscription_image", out var inscriptionImageUrl))
                // {
                //     nftCollectionInfoIndex.ImageUrl = inscriptionImageUrl;
                // }
                //
                // if (eventValue.ExternalInfo.Value.TryGetValue("__inscription_deploy", out var inscriptionDeploy))
                // {
                //     var inscriptionDeployMap =
                //         JsonConvert.DeserializeObject<Dictionary<string, string>>(inscriptionDeploy);
                //     if (inscriptionDeployMap.TryGetValue("tick", out var tick))
                //     {
                //         nftCollectionInfoIndex.InscriptionName = tick;
                //     }
                //
                //     if (inscriptionDeployMap.TryGetValue("lim", out var lim))
                //     {
                //         nftCollectionInfoIndex.Lim = lim;
                //     }
                // }
                // if (eventValue.ExternalInfo.Value.TryGetValue("inscription_deploy", out var inscriptionDeployInfo))
                // {
                //     var inscriptionDeployMap =
                //         JsonConvert.DeserializeObject<Dictionary<string, string>>(inscriptionDeployInfo);
                //     if (inscriptionDeployMap.TryGetValue("tick", out var tick))
                //     {
                //         nftCollectionInfoIndex.InscriptionName = tick;
                //     }
                //
                //     if (inscriptionDeployMap.TryGetValue("lim", out var lim))
                //     {
                //         nftCollectionInfoIndex.Lim = lim;
                //     }
                // }
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
                // nftInfoIndex.ExternalInfoDictionary = eventValue.ExternalInfo.Value
                //     .Where(t => !t.Key.IsNullOrWhiteSpace())
                //     .ToDictionary(item => item.Key, item => item.Value);
                //
                //
                // if (eventValue.ExternalInfo.Value.TryGetValue("__nft_image_url", out var imageUrl))
                // {
                //     nftInfoIndex.ImageUrl = imageUrl;
                // }
                // else if (eventValue.ExternalInfo.Value.TryGetValue("inscription_image", out var inscriptionImage))
                // {
                //     nftInfoIndex.ImageUrl = inscriptionImage;
                // }
                // else if(eventValue.ExternalInfo.Value.TryGetValue("__inscription_image", out var inscriptionImageUrl))
                // {
                //     nftInfoIndex.ImageUrl = inscriptionImageUrl;
                // }
                //
                //
                // if (eventValue.ExternalInfo.Value.TryGetValue("__nft_attributes", out var attributes))
                // {
                //     nftInfoIndex.Traits = attributes;
                // }
                //
                // if (eventValue.ExternalInfo.Value.TryGetValue("__seed_owned_symbol", out var seedOwnedSymbol))
                // {
                //     nftInfoIndex.SeedOwnedSymbol = seedOwnedSymbol;
                // }
                //
                // if (eventValue.ExternalInfo.Value.TryGetValue("__seed_exp_time", out var seedExpTime))
                // {
                //     nftInfoIndex.Expires = seedExpTime;
                // }
                //
                // if (eventValue.ExternalInfo.Value.TryGetValue("__inscription_adopt", out var inscriptionAdopt))
                // {
                //     var inscriptionDeployMap =
                //         JsonConvert.DeserializeObject<Dictionary<string, string>>(inscriptionAdopt);
                //     Log.Logger.Debug("Current Inscription Adopt: {inscriptionDeploy}", inscriptionDeployMap);
                //     if (inscriptionDeployMap.TryGetValue("gen", out var gen))
                //     {
                //         nftInfoIndex.Generation = gen;
                //     }
                //     if (inscriptionDeployMap.TryGetValue("tick", out var tick))
                //     {
                //         nftInfoIndex.InscriptionName = tick;
                //     }
                // }
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