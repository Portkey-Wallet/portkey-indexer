using AElf.CSharp.Core;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Portkey.Indexer.CA.Provider;
using Serilog;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public abstract class CAHolderTokenBalanceProcessorBase<TEvent> : CAHolderTransactionProcessorBase<TEvent>
    where TEvent : IEvent<TEvent>, new()
{
    protected IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> CAHolderIndexRepository;

    protected IAElfIndexerClientEntityRepository<CAHolderTokenBalanceIndex, TransactionInfo>
        CAHolderTokenBalanceIndexRepository;

    protected IAElfIndexerClientEntityRepository<CAHolderNFTCollectionBalanceIndex, TransactionInfo>
        CAHolderNFTCollectionBalanceRepository;

    protected IAElfIndexerClientEntityRepository<CAHolderNFTBalanceIndex, TransactionInfo>
        CAHolderNFTBalanceIndexRepository;

    protected IAElfIndexerClientEntityRepository<TokenInfoIndex, TransactionInfo> TokenInfoIndexRepository;
    protected IAElfIndexerClientEntityRepository<NFTCollectionInfoIndex, TransactionInfo> NftCollectionInfoRepository;
    protected IAElfIndexerClientEntityRepository<NFTInfoIndex, TransactionInfo> NftInfoRepository;

    private IAElfIndexerClientEntityRepository<CAHolderSearchTokenNFTIndex, TransactionInfo>
        CAHolderSearchTokenNFTRepository;

    protected IAElfDataProvider AElfDataProvider;
    protected readonly IObjectMapper ObjectMapper;
    protected readonly ContractInfoOptions ContractInfoOptions;
    protected readonly InscriptionListOptions _inscriptionListOptions;
    
    public CAHolderTokenBalanceProcessorBase(ILogger<CAHolderTokenBalanceProcessorBase<TEvent>> logger,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, TransactionInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTCollectionInfoIndex, TransactionInfo> nftCollectionInfoRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, TransactionInfo> nftInfoRepository,
        IAElfIndexerClientEntityRepository<CAHolderSearchTokenNFTIndex, TransactionInfo> caHolderSearchTokenNFTRepository,
        IAElfIndexerClientEntityRepository<CAHolderTokenBalanceIndex, TransactionInfo>
            caHolderTokenBalanceIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderNFTCollectionBalanceIndex, TransactionInfo>
            caHolderNFTCollectionBalanceRepository,
        IAElfIndexerClientEntityRepository<CAHolderNFTBalanceIndex, TransactionInfo> caHolderNFTBalanceIndexRepository,
        IAElfDataProvider aelfDataProvider,
        IObjectMapper objectMapper,
        IOptionsSnapshot<InscriptionListOptions> inscriptionListOptions,
        // transactionIndex needs
        IAElfIndexerClientEntityRepository<CAHolderManagerIndex, TransactionInfo> caHolderManagerIndexRepository = null,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo> caHolderTransactionIndexRepository = null,
        IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo> caHolderTransactionAddressIndexRepository = null,
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions = null) : base(logger, 
        caHolderIndexRepository, caHolderManagerIndexRepository, caHolderTransactionIndexRepository,
        tokenInfoIndexRepository, nftInfoRepository, caHolderTransactionAddressIndexRepository,
        contractInfoOptions, caHolderTransactionInfoOptions, objectMapper, aelfDataProvider)
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
            var tokenInfoIndex = await TokenInfoIndexRepository.GetFromBlockStateSetAsync(
                IdGenerateHelper.GetId(context.ChainId, symbol),
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
            var collectionBalance =
                await CAHolderNFTCollectionBalanceRepository.GetFromBlockStateSetAsync(id, context.ChainId);
            if (collectionBalance == null)
            {
                collectionBalance = new CAHolderNFTCollectionBalanceIndex
                {
                    Id = id,
                    NftCollectionInfo = nftCollectionInfo,
                    CAAddress = address,
                    TokenIds = new List<long>() { }
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

            if (_inscriptionListOptions.Inscriptions.Contains(nftInfo.CollectionSymbol) &&
                nftInfo.InscriptionName.IsNullOrWhiteSpace())
            {
                Log.Logger.Debug("nftInfo.CollectionSymbol: {0},nftInfo.InscriptionName:{1}", nftInfo.CollectionSymbol,
                    nftInfo.InscriptionName);

                if (nftCollectionInfo.InscriptionName.IsNullOrWhiteSpace())
                {
                    ObjectMapper.Map(context, nftCollectionInfo);
                    await UpdateCollectionInfoFromChainAsync(nftCollectionInfo);
                }

                nftInfo.InscriptionName = nftCollectionInfo.InscriptionName;
                nftInfo.Lim = nftCollectionInfo.Lim;
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
            var collectionBalance =
                await CAHolderNFTCollectionBalanceRepository.GetFromBlockStateSetAsync(collectionBalanceIndexId,
                    context.ChainId);
            if (collectionBalance == null)
            {
                collectionBalance = new CAHolderNFTCollectionBalanceIndex
                {
                    Id = collectionBalanceIndexId,
                    CAAddress = address,
                    TokenIds = new List<long>() { nftItemId }
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
        var collectionInfo =
            await AElfDataProvider.GetTokenInfoAsync(collectionInfoIndex.ChainId, collectionInfoIndex.Symbol);
        if (collectionInfo.Symbol == collectionInfoIndex.Symbol)
        {
            ObjectMapper.Map(collectionInfo, collectionInfoIndex);
            if (collectionInfo.ExternalInfo is { Count: > 0 })
            {
                var externalDictionary = collectionInfo.ExternalInfo;
                var externalInfo = NftExternalInfoHelper.BuildNftExternalInfo(externalDictionary);
                ObjectMapper.Map(externalInfo, collectionInfoIndex);
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
                var externalDictionary = nftInfo.ExternalInfo;
                var externalInfo = NftExternalInfoHelper.BuildNftExternalInfo(externalDictionary);
                ObjectMapper.Map(externalInfo, nftInfoIndex);
            }

            nftInfoIndex.ExternalInfoDictionary ??= new Dictionary<string, string>();
            await NftInfoRepository.AddOrUpdateAsync(nftInfoIndex);
        }
    }

    private async Task ModifySearchBalanceAsync(string address, string symbol, long amount, LogEventContext context)
    {
        TokenType tokenType = TokenHelper.GetTokenType(symbol);
        if (tokenType == TokenType.NFTCollection)
        {
            return;
        }

        if (tokenType == TokenType.Token)
        {
            //get token info from token index
            var tokenInfoIndex =
                await TokenInfoIndexRepository.GetFromBlockStateSetAsync(
                    IdGenerateHelper.GetId(context.ChainId, symbol),
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
                    TokenInfo = ObjectMapper.Map<TokenInfoIndex, TokenSearchInfo>(tokenInfoIndex)
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
                    NftInfo = ObjectMapper.Map<NFTInfoIndex, NFTSearchInfo>(nftInfoIndex)
                };
            }

            ObjectMapper.Map(context, caHolderSearchTokenNFTIndex);
            await CAHolderSearchTokenNFTRepository.AddOrUpdateAsync(caHolderSearchTokenNFTIndex);
        }
    }
}