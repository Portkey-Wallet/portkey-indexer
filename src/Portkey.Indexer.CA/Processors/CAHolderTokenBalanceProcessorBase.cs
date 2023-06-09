using AElf.CSharp.Core;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Portkey.Indexer.CA.GraphQL;
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
    protected readonly IObjectMapper ObjectMapper;
    protected readonly ContractInfoOptions ContractInfoOptions;

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
        IObjectMapper objectMapper) : base(logger)
    {
        CAHolderIndexRepository = caHolderIndexRepository;
        CAHolderTokenBalanceIndexRepository = caHolderTokenBalanceIndexRepository;
        CAHolderNFTCollectionBalanceRepository = caHolderNFTCollectionBalanceRepository;
        CAHolderNFTBalanceIndexRepository = caHolderNFTBalanceIndexRepository;
        ObjectMapper = objectMapper;
        TokenInfoIndexRepository = tokenInfoIndexRepository;
        NftCollectionInfoRepository = nftCollectionInfoRepository;
        NftInfoRepository = nftInfoRepository;
        CAHolderSearchTokenNFTRepository = caHolderSearchTokenNFTRepository;
        ContractInfoOptions = contractInfoOptions.Value;
    }
    
    protected async Task ModifyBalanceAsync(string address, string symbol, long amount, LogEventContext context)
    {
        TokenType tokenType = TokenHelper.GetTokenType(symbol);
        if (tokenType == TokenType.Token)
        {
            var tokenInfo =
                await TokenInfoIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId, symbol),
                    context.ChainId);
            var id = IdGenerateHelper.GetId(context.ChainId, address, symbol);
            var tokenBalance = await CAHolderTokenBalanceIndexRepository.GetFromBlockStateSetAsync(id, context.ChainId);
            if (tokenBalance == null)
            {
                tokenBalance = new CAHolderTokenBalanceIndex
                {
                    Id = id,
                    TokenInfo = tokenInfo,
                    CAAddress = address
                };
            }

            tokenBalance.Balance += amount;
            ObjectMapper.Map(context, tokenBalance);
            
            await CAHolderTokenBalanceIndexRepository.AddOrUpdateAsync(tokenBalance);
        }

        if (tokenType == TokenType.NFTCollection)
        {
            var nftCollectionInfo =
                await NftCollectionInfoRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId, symbol),
                    context.ChainId);
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
            var nftInfo =
                await NftInfoRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId, symbol),
                    context.ChainId);
            var id = IdGenerateHelper.GetId(context.ChainId, address, symbol);
            var nftBalance = await CAHolderNFTBalanceIndexRepository.GetFromBlockStateSetAsync(id, context.ChainId);
            if (nftBalance == null)
            {
                nftBalance = new CAHolderNFTBalanceIndex
                {
                    Id = id,
                    NftInfo = nftInfo,
                    CAAddress = address
                };
            }

            nftBalance.Balance += amount;
            ObjectMapper.Map(context, nftBalance);
            await CAHolderNFTBalanceIndexRepository.AddOrUpdateAsync(nftBalance);
            
            var nftItemId = TokenHelper.GetNFTItemId(symbol);
            var nftCollectionSymbol = TokenHelper.GetNFTCollectionSymbol(symbol);
            var collectionBalanceIndexId = IdGenerateHelper.GetId(context.ChainId, address, nftCollectionSymbol);
            var collectionBalance = await CAHolderNFTCollectionBalanceRepository.GetFromBlockStateSetAsync(collectionBalanceIndexId,context.ChainId);
            if (collectionBalance == null)
            {
                var collectionInfoIndexId = IdGenerateHelper.GetId(context.ChainId, nftCollectionSymbol);
                var collectionInfoIndex =
                    await NftCollectionInfoRepository.GetFromBlockStateSetAsync(collectionInfoIndexId, context.ChainId);
                collectionBalance = new CAHolderNFTCollectionBalanceIndex
                {
                    Id = collectionBalanceIndexId,
                    NftCollectionInfo = collectionInfoIndex,
                    CAAddress = address,
                    TokenIds=new List<long>() { nftItemId }
                };
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