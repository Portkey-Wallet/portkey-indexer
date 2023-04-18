using AElf.CSharp.Core;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public abstract class CAHolderNFTBalanceProcessorBase<TEvent> : AElfLogEventProcessorBase<TEvent,LogEventInfo> where TEvent : IEvent<TEvent>, new()
{
    protected IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> CAHolderIndexRepository;
    private IAElfIndexerClientEntityRepository<NFTInfoIndex, LogEventInfo> _nftInfoIndexRepository;
    private IAElfIndexerClientEntityRepository<CAHolderSearchTokenNFTIndex, LogEventInfo> _caHolderSearchTokenNFTRepository;
    protected readonly IObjectMapper ObjectMapper;
    protected readonly ContractInfoOptions ContractInfoOptions;

    public CAHolderNFTBalanceProcessorBase(ILogger<CAHolderNFTBalanceProcessorBase<TEvent>> logger,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, LogEventInfo> nftInfoIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderSearchTokenNFTIndex, LogEventInfo> caHolderSearchTokenNFTRepository,
        IObjectMapper objectMapper) : base(logger)
    {
        CAHolderIndexRepository = caHolderIndexRepository;
        ObjectMapper = objectMapper;
        _nftInfoIndexRepository = nftInfoIndexRepository;
        _caHolderSearchTokenNFTRepository = caHolderSearchTokenNFTRepository;
        ContractInfoOptions = contractInfoOptions.Value;
    }

    protected async Task ModifyBalanceAsync(string address, string symbol, long tokenId, long amount,
        LogEventContext context)
    {
        var holder = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            address),context.ChainId);
        if (holder == null) return;
        
        //Update Search index Balance
        await ModifySearchBalanceAsync(address, symbol, tokenId, amount, context);
    }

    private async Task ModifySearchBalanceAsync(string address, string symbol, long tokenId, long amount,
        LogEventContext context)
    {
        //check NFT info index is exist
        var nftInfoId = IdGenerateHelper.GetId(context.ChainId, symbol, tokenId);
        var nftInfoIndex = await _nftInfoIndexRepository.GetFromBlockStateSetAsync(nftInfoId, context.ChainId);
        if (nftInfoIndex == null) return;
        
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
                // NFTInfo = ObjectMapper.Map<NFTInfoIndex, NFTSearchInfo>(nftInfoIndex)
            };
        }
        ObjectMapper.Map(context, caHolderSearchTokenNFTIndex);
        await _caHolderSearchTokenNFTRepository.AddOrUpdateAsync(caHolderSearchTokenNFTIndex);
        
    }
}