using AElf.CSharp.Core;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Portkey.Indexer.CA.Entities;
using Portkey.Indexer.CA.Provider;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public abstract class CAHolderTokenBalanceProcessorBase<TEvent> : AElfLogEventProcessorBase<TEvent,LogEventInfo> where TEvent : IEvent<TEvent>, new()
{
    protected IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> CAHolderIndexRepository;
    protected IAElfIndexerClientEntityRepository<CAHolderTokenBalanceIndex, LogEventInfo> CAHolderTokenBalanceIndexRepository;
    protected IAElfIndexerClientEntityRepository<CAHolderNFTCollectionBalanceIndex, LogEventInfo> CAHolderNFTCollectionBalanceRepository;
    protected IAElfIndexerClientEntityRepository<CAHolderNFTBalanceIndex, LogEventInfo> CAHolderNFTBalanceIndexRepository;
    protected IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> TokenInfoIndexRepository;
    protected IAElfIndexerClientEntityRepository<NFTCollectionInfoIndex, LogEventInfo> NftCollectionInfoRepository;
    protected IAElfIndexerClientEntityRepository<NFTInfoIndex, LogEventInfo> NftInfoRepository;
    private IAElfIndexerClientEntityRepository<CAHolderSearchTokenNFTIndex, LogEventInfo> CAHolderSearchTokenNFTRepository;
    protected IAElfDataProvider AElfDataProvider;
    protected readonly IObjectMapper ObjectMapper;
    protected readonly ContractInfoOptions ContractInfoOptions;
    protected readonly InscriptionListOptions _inscriptionListOptions;

    public CAHolderTokenBalanceProcessorBase(ILogger<CAHolderTokenBalanceProcessorBase<TEvent>> logger,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTCollectionInfoIndex, LogEventInfo> nftCollectionInfoRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, LogEventInfo> nftInfoRepository,
        IAElfIndexerClientEntityRepository<CAHolderSearchTokenNFTIndex, LogEventInfo> caHolderSearchTokenNFTRepository,
        IAElfIndexerClientEntityRepository<CAHolderTokenBalanceIndex, LogEventInfo>
            caHolderTokenBalanceIndexRepository, 
        IAElfIndexerClientEntityRepository<CAHolderNFTCollectionBalanceIndex, LogEventInfo> caHolderNFTCollectionBalanceRepository,
        IAElfIndexerClientEntityRepository<CAHolderNFTBalanceIndex, LogEventInfo> caHolderNFTBalanceIndexRepository,
        IAElfDataProvider aelfDataProvider,
        IObjectMapper objectMapper, IOptionsSnapshot<InscriptionListOptions> inscriptionListOptions) : base(logger)
    {
        CAHolderIndexRepository = caHolderIndexRepository;
        CAHolderTokenBalanceIndexRepository = caHolderTokenBalanceIndexRepository;
        CAHolderNFTCollectionBalanceRepository = caHolderNFTCollectionBalanceRepository;
        CAHolderNFTBalanceIndexRepository = caHolderNFTBalanceIndexRepository;
        ObjectMapper = objectMapper;
        _inscriptionListOptions = inscriptionListOptions.Value;
        TokenInfoIndexRepository = tokenInfoIndexRepository;
        NftCollectionInfoRepository = nftCollectionInfoRepository;
        NftInfoRepository = nftInfoRepository;
        CAHolderSearchTokenNFTRepository = caHolderSearchTokenNFTRepository;
        ContractInfoOptions = contractInfoOptions.Value;
        AElfDataProvider = aelfDataProvider;
    }
    
    protected async Task ModifyBalanceAsync(string address, string symbol, long amount, LogEventContext context)
    {
        TokenType tokenType = TokenHelper.GetTokenType(symbol);
        if (tokenType == TokenType.Token)
        {
            var tokenInfoIndex = await TokenInfoIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId, symbol),
                context.ChainId);
            if (tokenInfoIndex == null)
            {
                tokenInfoIndex = new TokenInfoIndex
                {
                    Id = IdGenerateHelper.GetId(context.ChainId, symbol),
                    TokenContractAddress = GetContractAddress(context.ChainId),
                    Type = TokenType.Token,
                    Symbol = symbol
                };
                ObjectMapper.Map(context, tokenInfoIndex);
                await UpdateTokenInfoFromChainAsync(tokenInfoIndex);
            }
            
            var id = IdGenerateHelper.GetId(context.ChainId, address, symbol);
            var tokenBalance = await CAHolderTokenBalanceIndexRepository.GetFromBlockStateSetAsync(id, context.ChainId);
            if (tokenBalance == null)
            {
                tokenBalance = new CAHolderTokenBalanceIndex
                {
                    Id = id,
                    TokenInfo = tokenInfoIndex,
                    CAAddress = address
                };
            }

            if (tokenBalance.TokenInfo == null)
            {
                tokenBalance.TokenInfo = tokenInfoIndex;
            }

            tokenBalance.Balance += amount;
            ObjectMapper.Map(context, tokenBalance);
            
            await CAHolderTokenBalanceIndexRepository.AddOrUpdateAsync(tokenBalance);
        }

        NFTCollectionInfoIndex nftCollectionInfo = null;
        if (tokenType == TokenType.NFTCollection || tokenType == TokenType.NFTItem)
        {
            var nftCollectionSymbol = TokenHelper.GetNFTCollectionSymbol(symbol);
            var nftCollectionInfoId = IdGenerateHelper.GetId(context.ChainId, nftCollectionSymbol);
            nftCollectionInfo =
                await NftCollectionInfoRepository.GetFromBlockStateSetAsync(nftCollectionInfoId,
                    context.ChainId);
            if (nftCollectionInfo == null)
            {
                nftCollectionInfo = new NFTCollectionInfoIndex
                {
                    Id = nftCollectionInfoId,
                    Symbol = nftCollectionSymbol,
                    Type = TokenType.NFTCollection,
                    TokenContractAddress = GetContractAddress(context.ChainId)
                };
                ObjectMapper.Map(context, nftCollectionInfo);
                await UpdateCollectionInfoFromChainAsync(nftCollectionInfo);
            }
        }

        if (tokenType == TokenType.NFTCollection)
        {
            var id = IdGenerateHelper.GetId(context.ChainId, address, symbol);
            var collectionBalance = await CAHolderNFTCollectionBalanceRepository.GetFromBlockStateSetAsync(id, context.ChainId);
            if (collectionBalance == null)
            {
                collectionBalance = new CAHolderNFTCollectionBalanceIndex
                {
                    Id = id,
                    NftCollectionInfo = nftCollectionInfo,
                    CAAddress = address,
                    TokenIds = new List<long>(){}
                };
            }

            collectionBalance.Balance += amount;
            ObjectMapper.Map(context, collectionBalance);
            
            await CAHolderNFTCollectionBalanceRepository.AddOrUpdateAsync(collectionBalance);
            
        }

        if (tokenType == TokenType.NFTItem)
        {
            var nftInfoId = IdGenerateHelper.GetId(context.ChainId, symbol);
            var nftInfo =
                await NftInfoRepository.GetFromBlockStateSetAsync(nftInfoId,
                    context.ChainId);
            if (nftInfo == null)
            {
                nftInfo = new NFTInfoIndex()
                {
                    Id = nftInfoId,
                    Symbol = symbol,
                    Type = TokenType.NFTItem,
                    TokenContractAddress = GetContractAddress(context.ChainId),
                    CollectionName = nftCollectionInfo.TokenName,
                    CollectionSymbol = nftCollectionInfo.Symbol
                };
                ObjectMapper.Map(context, nftInfo);
                await UpdateNftInfoFromChainAsync(nftInfo);
            }
            else if (string.IsNullOrWhiteSpace(nftInfo.CollectionSymbol))
            {
                nftInfo.CollectionName = nftCollectionInfo.TokenName;
                nftInfo.CollectionSymbol = nftCollectionInfo.Symbol;
                ObjectMapper.Map(context, nftInfo);
                await NftInfoRepository.AddOrUpdateAsync(nftInfo);
            }
            if (_inscriptionListOptions.Inscriptions.Contains(nftInfo.CollectionSymbol) && nftInfo.InscriptionName.IsNullOrWhiteSpace())
            {
                // var collectionInfo = await AElfDataProvider.GetTokenInfoAsync(context.ChainId, nftInfo.CollectionSymbol);
                // var collectionId = IdGenerateHelper.GetId(context.ChainId, address, nftInfo.CollectionSymbol);
                // var collectionDetail = await NftCollectionInfoRepository.GetFromBlockStateSetAsync(collectionId, context.ChainId);
                // if (collectionInfo.ExternalInfo.TryGetValue("inscription_deploy", out var inscriptionDeploy))
                // {
                //     var inscriptionDeployMap =
                //         JsonConvert.DeserializeObject<Dictionary<string, string>>(inscriptionDeploy);
                //     if (inscriptionDeployMap.TryGetValue("tick", out var tick))
                //     {
                //         nftInfo.InscriptionName = tick;
                //         collectionDetail.InscriptionName = tick;
                //     }
                //
                //     if (inscriptionDeployMap.TryGetValue("lim", out var lim))
                //     {
                //         nftInfo.LimitPerMint = int.Parse(lim);
                //         collectionDetail.LimitPerMint = int.Parse(lim);
                //     }
                // }
                // ObjectMapper.Map(context, nftInfo);
                // await NftInfoRepository.AddOrUpdateAsync(nftInfo);
                

                if (nftCollectionInfo.InscriptionName.IsNullOrWhiteSpace())
                {
                    ObjectMapper.Map(context, nftCollectionInfo);
                    await UpdateCollectionInfoFromChainAsync(nftCollectionInfo);
                }

                nftInfo.InscriptionName = nftCollectionInfo.InscriptionName;
                nftInfo.LimitPerMint = nftCollectionInfo.LimitPerMint;
                ObjectMapper.Map(context, nftInfo);
                await NftInfoRepository.AddOrUpdateAsync(nftInfo);
            }

            var id = IdGenerateHelper.GetId(context.ChainId, address, symbol);
            var nftBalance = await CAHolderNFTBalanceIndexRepository.GetFromBlockStateSetAsync(id, context.ChainId);
            if (nftBalance == null)
            {
                nftBalance = new CAHolderNFTBalanceIndex
                {
                    Id = id,
                    CAAddress = address
                };
            }
            if (nftBalance.NftInfo == null)
            {
                nftBalance.NftInfo = nftInfo;
            }

            if (string.IsNullOrWhiteSpace(nftBalance.NftInfo.CollectionSymbol))
            {
                nftBalance.NftInfo.CollectionSymbol = nftCollectionInfo.Symbol;
                nftBalance.NftInfo.CollectionName = nftCollectionInfo.TokenName;
            }

            nftBalance.Balance += amount;
            ObjectMapper.Map(context, nftBalance);
            await CAHolderNFTBalanceIndexRepository.AddOrUpdateAsync(nftBalance);
            
            var nftItemId = TokenHelper.GetNFTItemId(symbol);
            var collectionBalanceIndexId = IdGenerateHelper.GetId(context.ChainId, address, nftCollectionInfo.Symbol);
            var collectionBalance = await CAHolderNFTCollectionBalanceRepository.GetFromBlockStateSetAsync(collectionBalanceIndexId,context.ChainId);
            if (collectionBalance == null)
            {
                collectionBalance = new CAHolderNFTCollectionBalanceIndex
                {
                    Id = collectionBalanceIndexId,
                    CAAddress = address,
                    TokenIds=new List<long>() { nftItemId }
                };
            }

            if (collectionBalance.NftCollectionInfo == null)
            {
                collectionBalance.NftCollectionInfo = nftCollectionInfo;
            }

            if (collectionBalance.TokenIds == null)
            {
                collectionBalance.TokenIds = new List<long>() { };
            }
            if (amount < 0)
            {
                if (nftBalance.Balance == 0 &&
                    collectionBalance.TokenIds.Contains(nftItemId))
                {
                    collectionBalance.TokenIds.Remove(nftItemId);
                }
            }
            else
            {
                if (!collectionBalance.TokenIds.Contains(nftItemId))
                {
                    collectionBalance.TokenIds.Add(nftItemId);
                }
            }
            ObjectMapper.Map(context, collectionBalance);
            await CAHolderNFTCollectionBalanceRepository.AddOrUpdateAsync(collectionBalance);
        }

        //Update Search index Balance
        await ModifySearchBalanceAsync(address, symbol, amount, context);
    }
    
    private async Task UpdateTokenInfoFromChainAsync(TokenInfoIndex tokenInfoIndex)
    {
        var tokenInfoAsync = await AElfDataProvider.GetTokenInfoAsync(tokenInfoIndex.ChainId, tokenInfoIndex.Symbol);
        if (tokenInfoAsync.Symbol == tokenInfoIndex.Symbol)
        {
            ObjectMapper.Map(tokenInfoAsync, tokenInfoIndex);
            if (tokenInfoAsync.ExternalInfo is { Count: > 0 })
            {
                tokenInfoIndex.ExternalInfoDictionary = tokenInfoAsync.ExternalInfo
                    .Where(t => !t.Key.IsNullOrWhiteSpace())
                    .ToDictionary(item => item.Key, item => item.Value);
            }
            tokenInfoIndex.ExternalInfoDictionary ??= new Dictionary<string, string>();
            await TokenInfoIndexRepository.AddOrUpdateAsync(tokenInfoIndex);
        }
    }

    private async Task UpdateCollectionInfoFromChainAsync(NFTCollectionInfoIndex collectionInfoIndex)
    {
        var collectionInfo = await AElfDataProvider.GetTokenInfoAsync(collectionInfoIndex.ChainId, collectionInfoIndex.Symbol);
        if (collectionInfo.Symbol == collectionInfoIndex.Symbol)
        {
                        
            ObjectMapper.Map(collectionInfo, collectionInfoIndex);
            if (collectionInfo.ExternalInfo is { Count: > 0 })
            {
                collectionInfoIndex.ExternalInfoDictionary = collectionInfo.ExternalInfo
                    .Where(t => !t.Key.IsNullOrWhiteSpace())
                    .ToDictionary(item => item.Key, item => item.Value);
                if (collectionInfo.ExternalInfo.TryGetValue("__nft_image_url", out var imageUrl))
                {
                    collectionInfoIndex.ImageUrl = imageUrl;
                }
                else if (collectionInfo.ExternalInfo.TryGetValue("inscription_image", out var inscriptionImage))
                {
                    collectionInfoIndex.ImageUrl = inscriptionImage;
                }
                
                if (collectionInfo.ExternalInfo.TryGetValue("__inscription_deploy", out var inscriptionDeploy))
                {
                    var inscriptionDeployMap =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(inscriptionDeploy);
                    if (inscriptionDeployMap.TryGetValue("tick", out var tick))
                    {
                        collectionInfoIndex.InscriptionName = tick;
                    }

                    if (inscriptionDeployMap.TryGetValue("lim", out var lim))
                    {
                        collectionInfoIndex.LimitPerMint = int.Parse(lim);
                    }
                }
                if (collectionInfo.ExternalInfo.TryGetValue("inscription_deploy", out var inscriptionDeployInfo))
                {
                    var inscriptionDeployMap =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(inscriptionDeployInfo);
                    if (inscriptionDeployMap.TryGetValue("tick", out var tick))
                    {
                        collectionInfoIndex.InscriptionName = tick;
                    }

                    if (inscriptionDeployMap.TryGetValue("lim", out var lim))
                    {
                        collectionInfoIndex.LimitPerMint = int.Parse(lim);
                    }
                }

               
            }
            collectionInfoIndex.ExternalInfoDictionary ??= new Dictionary<string, string>();
            await NftCollectionInfoRepository.AddOrUpdateAsync(collectionInfoIndex);
        }
    }

    private async Task UpdateNftInfoFromChainAsync(NFTInfoIndex nftInfoIndex)
    {
        var nftInfo = await AElfDataProvider.GetTokenInfoAsync(nftInfoIndex.ChainId, nftInfoIndex.Symbol);
        if (nftInfo.Symbol == nftInfoIndex.Symbol)
        {
            ObjectMapper.Map(nftInfo, nftInfoIndex);
            if (nftInfo.ExternalInfo is { Count: > 0 })
            {
                nftInfoIndex.ExternalInfoDictionary = nftInfo.ExternalInfo
                    .Where(t => !t.Key.IsNullOrWhiteSpace())
                    .ToDictionary(item => item.Key, item => item.Value);
                if (nftInfo.ExternalInfo.TryGetValue("__nft_image_url", out var imageUrl))
                {
                    nftInfoIndex.ImageUrl = imageUrl;
                }
                else if (nftInfo.ExternalInfo.TryGetValue("inscription_image", out var inscriptionImage))
                {
                    nftInfoIndex.ImageUrl = inscriptionImage;
                }
                if (nftInfo.ExternalInfo.TryGetValue("__nft_attributes", out var nftAttributes))
                {
                    nftInfoIndex.Straits = nftAttributes;
                }
                if (nftInfo.ExternalInfo.TryGetValue("__inscription_adopt", out var inscriptionAdopt))
                {
                    var inscriptionDeployMap =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(inscriptionAdopt);
                    if (inscriptionDeployMap.TryGetValue("gen", out var gen))
                    {
                        nftInfoIndex.Generation = gen;
                    }

                    if (inscriptionDeployMap.TryGetValue("tick", out var tick))
                    {
                        nftInfoIndex.InscriptionName = tick;
                    }
                }

                if (nftInfo.ExternalInfo.TryGetValue("__seed_owned_symbol", out var seedOwnedSymbol))
                {
                    nftInfoIndex.SeedOwnedSymbol = seedOwnedSymbol;
                }

                if (nftInfo.ExternalInfo.TryGetValue("__seed_exp_time", out var seedExpTime))
                {
                    nftInfoIndex.Expires = seedExpTime;
                }
                
                
            }
            nftInfoIndex.ExternalInfoDictionary ??= new Dictionary<string, string>();
            await NftInfoRepository.AddOrUpdateAsync(nftInfoIndex);
        }
    }

    private async Task ModifySearchBalanceAsync(string address, string symbol, long amount, LogEventContext context)
    {
        // //if symbol has been existed in NFT protocol, then do nothing
        // var nftProtocolInfoIndex =
        //     await _nftProtocolInfoRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId, symbol),
        //         context.ChainId);
        // if (nftProtocolInfoIndex != null)
        // {
        //     return;
        // }
        
        TokenType tokenType = TokenHelper.GetTokenType(symbol);
        if (tokenType == TokenType.NFTCollection)
        {
            return;
        }

        if (tokenType == TokenType.Token)
        {
            //get token info from token index
            var tokenInfoIndex =
                await TokenInfoIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId, symbol),
                    context.ChainId);

            var id = IdGenerateHelper.GetId(context.ChainId, address, symbol);
            var caHolderSearchTokenNFTIndex =
                await CAHolderSearchTokenNFTRepository.GetFromBlockStateSetAsync(id, context.ChainId);
            if (caHolderSearchTokenNFTIndex != null)
            {
                caHolderSearchTokenNFTIndex.Balance += amount;
            }
            else
            {
                caHolderSearchTokenNFTIndex = new CAHolderSearchTokenNFTIndex()
                {
                    Id = IdGenerateHelper.GetId(context.ChainId, address, symbol),
                    CAAddress = address,
                    Balance = amount,
                    TokenInfo = ObjectMapper.Map<TokenInfoIndex,TokenSearchInfo>(tokenInfoIndex)
                };
            }
            ObjectMapper.Map(context, caHolderSearchTokenNFTIndex);
            await CAHolderSearchTokenNFTRepository.AddOrUpdateAsync(caHolderSearchTokenNFTIndex);
        }

        if (tokenType == TokenType.NFTItem)
        {
            //get nft info from nft index
            var nftInfoIndex =
                await NftInfoRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId, symbol),
                    context.ChainId);

            var id = IdGenerateHelper.GetId(context.ChainId, address, symbol);
            var caHolderSearchTokenNFTIndex =
                await CAHolderSearchTokenNFTRepository.GetFromBlockStateSetAsync(id, context.ChainId);
            if (caHolderSearchTokenNFTIndex != null)
            {
                caHolderSearchTokenNFTIndex.Balance += amount;
            }
            else
            {
                caHolderSearchTokenNFTIndex = new CAHolderSearchTokenNFTIndex()
                {
                    Id = IdGenerateHelper.GetId(context.ChainId, address, symbol),
                    CAAddress = address,
                    Balance = amount,
                    TokenId = TokenHelper.GetNFTItemId(symbol),
                    NftInfo = ObjectMapper.Map<NFTInfoIndex,NFTSearchInfo>(nftInfoIndex)
                };
            }
            ObjectMapper.Map(context, caHolderSearchTokenNFTIndex);
            await CAHolderSearchTokenNFTRepository.AddOrUpdateAsync(caHolderSearchTokenNFTIndex);
        }
        
        
    }
}