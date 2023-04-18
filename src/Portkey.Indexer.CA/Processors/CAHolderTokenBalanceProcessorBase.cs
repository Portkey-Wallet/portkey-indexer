using AElf.CSharp.Core;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public abstract class CAHolderTokenBalanceProcessorBase<TEvent> : AElfLogEventProcessorBase<TEvent,LogEventInfo> where TEvent : IEvent<TEvent>, new()
{
    protected IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> CAHolderIndexRepository;
    private IAElfIndexerClientEntityRepository<CAHolderTokenBalanceIndex, LogEventInfo> _caHolderTokenBalanceIndexRepository;
    private IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> _tokenInfoIndexRepository;
    private IAElfIndexerClientEntityRepository<NFTProtocolInfoIndex, LogEventInfo> _nftProtocolInfoRepository;
    private IAElfIndexerClientEntityRepository<CAHolderSearchTokenNFTIndex, LogEventInfo> _caHolderSearchTokenNFTRepository;
    protected readonly IObjectMapper ObjectMapper;
    protected readonly ContractInfoOptions ContractInfoOptions;

    public CAHolderTokenBalanceProcessorBase(ILogger<CAHolderTokenBalanceProcessorBase<TEvent>> logger,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<NFTProtocolInfoIndex, LogEventInfo> nftProtocolInfoRepository,
        IAElfIndexerClientEntityRepository<CAHolderSearchTokenNFTIndex, LogEventInfo> caHolderSearchTokenNFTRepository,
        IAElfIndexerClientEntityRepository<CAHolderTokenBalanceIndex, LogEventInfo>
            caHolderTokenBalanceIndexRepository, IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> tokenInfoIndexRepository,IObjectMapper objectMapper) : base(logger)
    {
        CAHolderIndexRepository = caHolderIndexRepository;
        _caHolderTokenBalanceIndexRepository = caHolderTokenBalanceIndexRepository;
        ObjectMapper = objectMapper;
        _tokenInfoIndexRepository = tokenInfoIndexRepository;
        _nftProtocolInfoRepository = nftProtocolInfoRepository;
        _caHolderSearchTokenNFTRepository = caHolderSearchTokenNFTRepository;
        ContractInfoOptions = contractInfoOptions.Value;
    }
    
    protected async Task ModifyBalanceAsync(string address, string symbol, long amount, LogEventContext context)
    {
        var id = IdGenerateHelper.GetId(context.ChainId, address, symbol);
        var tokenInfo =
            await _tokenInfoIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId, symbol),
                context.ChainId);
        var tokenBalance = await _caHolderTokenBalanceIndexRepository.GetFromBlockStateSetAsync(id, context.ChainId);
        if (tokenBalance == null)
        {
            tokenBalance = new CAHolderTokenBalanceIndex
            {
                Id = id,
                TokenInfo = new Entities.TokenInfo
                {
                    Decimals = tokenInfo.Decimals,
                    Symbol = tokenInfo.Symbol
                },
                CAAddress = address
            };
        }

        tokenBalance.Balance += amount;
        ObjectMapper.Map(context, tokenBalance);
        await _caHolderTokenBalanceIndexRepository.AddOrUpdateAsync(tokenBalance);
        
        //Update Search index Balance
        await ModifySearchBalanceAsync(address, symbol, amount, context);
    }

    private async Task ModifySearchBalanceAsync(string address, string symbol, long amount, LogEventContext context)
    {
        //if symbol has been existed in NFT protocol, then do nothing
        var nftProtocolInfoIndex =
            await _nftProtocolInfoRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId, symbol),
                context.ChainId);
        if (nftProtocolInfoIndex != null)
        {
            return;
        }
        //get token info from token index
        var tokenInfoIndex =
            await _tokenInfoIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId, symbol),
                context.ChainId);

        var id = IdGenerateHelper.GetId(context.ChainId, address, symbol);
        var caHolderSearchTokenNFTIndex =
            await _caHolderSearchTokenNFTRepository.GetFromBlockStateSetAsync(id, context.ChainId);
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
        await _caHolderSearchTokenNFTRepository.AddOrUpdateAsync(caHolderSearchTokenNFTIndex);
    }
}