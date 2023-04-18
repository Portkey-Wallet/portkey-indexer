using AElf.Contracts.NFT;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class NFTTransferredLogEventProcessor :  CAHolderNFTBalanceProcessorBase<Transferred>
{
    private readonly IAElfIndexerClientEntityRepository<UserNFTInfoIndex, LogEventInfo> _userNFTInfoIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<UserNFTProtocolInfoIndex, LogEventInfo> _userNFTProtocolIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<NFTProtocolInfoIndex, LogEventInfo> _nftProtocolInfoRepository;
    private readonly IAElfIndexerClientEntityRepository<NFTInfoIndex, LogEventInfo> _nftInfoIndexRepository;

    public NFTTransferredLogEventProcessor(ILogger<NFTTransferredLogEventProcessor> logger,
        IAElfIndexerClientEntityRepository<UserNFTInfoIndex, LogEventInfo> userNFTInfoIndexRepository,
        IAElfIndexerClientEntityRepository<UserNFTProtocolInfoIndex, LogEventInfo> userNFTProtocolIndexRepository,
        IAElfIndexerClientEntityRepository<NFTProtocolInfoIndex, LogEventInfo> nftProtocolInfoRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, LogEventInfo> nftInfoIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderSearchTokenNFTIndex, LogEventInfo> caHolderSearchTokenNFTRepository,
        IObjectMapper objectMapper, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> caHolderIndexRepository)
        : base(logger, contractInfoOptions, caHolderIndexRepository, nftInfoIndexRepository,
            caHolderSearchTokenNFTRepository, objectMapper)
    {
        _userNFTInfoIndexRepository = userNFTInfoIndexRepository;
        _userNFTProtocolIndexRepository = userNFTProtocolIndexRepository;
        _nftProtocolInfoRepository = nftProtocolInfoRepository;
        _nftInfoIndexRepository = nftInfoIndexRepository;
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).NFTContractAddress;
    }
    
    protected override async Task HandleEventAsync(Transferred eventValue, LogEventContext context)
    {
        //deal with From address balance
        var infoIndexId = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol, eventValue.TokenId, eventValue.From.ToBase58());
        var userNFTInfoIndex_from =
            await _userNFTInfoIndexRepository.GetFromBlockStateSetAsync(infoIndexId, context.ChainId);
        if (userNFTInfoIndex_from != null)
        {
            ObjectMapper.Map(context, userNFTInfoIndex_from);
            userNFTInfoIndex_from.Balance -= eventValue.Amount;
            
            await _userNFTInfoIndexRepository.AddOrUpdateAsync(userNFTInfoIndex_from);
            
            await ModifyBalanceAsync(eventValue.From.ToBase58(), eventValue.Symbol, eventValue.TokenId,
                -eventValue.Amount, context);
        }

        //deal with From address's nft protocol info
        var protocolIndexId_from = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol,
            eventValue.From.ToBase58());
        var userNFTProtocolInfoIndex_from = await _userNFTProtocolIndexRepository.GetFromBlockStateSetAsync(protocolIndexId_from,context.ChainId);
        if (userNFTProtocolInfoIndex_from != null)
        {
            ObjectMapper.Map(context, userNFTProtocolInfoIndex_from);
            //TODO check Quantity == 0 ,remove TokenIds
            if (userNFTInfoIndex_from.Balance == 0 && userNFTProtocolInfoIndex_from.TokenIds.Contains(eventValue.TokenId))
            {
                userNFTProtocolInfoIndex_from.TokenIds.Remove(eventValue.TokenId);
            }
            await _userNFTProtocolIndexRepository.AddOrUpdateAsync(userNFTProtocolInfoIndex_from);
        }

        //deal with To address balance
        infoIndexId = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol, eventValue.TokenId, eventValue.To.ToBase58());
        var userNFTInfoIndex_to =
            await _userNFTInfoIndexRepository.GetFromBlockStateSetAsync(infoIndexId, context.ChainId);
        if (userNFTInfoIndex_to != null)
        {
            userNFTInfoIndex_to.Balance += eventValue.Amount;
        }
        else
        {
            var caHolder =
                await CAHolderIndexRepository.GetFromBlockStateSetAsync(
                    IdGenerateHelper.GetId(context.ChainId, eventValue.To.ToBase58()), context.ChainId);
            if (caHolder == null) return;
            var nftInfoIndex = await GetNFTInfoIndex(eventValue, context);
            userNFTInfoIndex_to = new UserNFTInfoIndex()
            {
                Id = infoIndexId,
                CAAddress = caHolder.CAAddress,
                Balance = eventValue.Amount,
                NftInfo = ObjectMapper.Map<NFTInfoIndex, NFTItemInfo>(nftInfoIndex)
            };
        }
        ObjectMapper.Map(context, userNFTInfoIndex_to);
        await _userNFTInfoIndexRepository.AddOrUpdateAsync(userNFTInfoIndex_to);
        
        await ModifyBalanceAsync(eventValue.To.ToBase58(), eventValue.Symbol, eventValue.TokenId,
            eventValue.Amount, context);
        
        //deal with to address's nft protocol info
        var protocolIndexId_to = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol,
            eventValue.To.ToBase58());
        var userNFTProtocolInfoIndex_to = await _userNFTProtocolIndexRepository.GetFromBlockStateSetAsync(protocolIndexId_to,context.ChainId);
        if (userNFTProtocolInfoIndex_to != null)
        {
            if (!userNFTProtocolInfoIndex_to.TokenIds.Contains(eventValue.TokenId))
            {
                userNFTProtocolInfoIndex_to.TokenIds.Add(eventValue.TokenId);
            }
        }
        else
        {
            var nftProtocolInfoIndex = await GetNFTProtocolInfoIndex(eventValue, context);
            userNFTProtocolInfoIndex_to = new UserNFTProtocolInfoIndex()
            {
                Id = protocolIndexId_to,
                CAAddress = eventValue.To.ToBase58(),
                TokenIds=new List<long>() { eventValue.TokenId },
                NftProtocolInfo = ObjectMapper.Map<NFTProtocolInfoIndex, NFTProtocol>(nftProtocolInfoIndex)
            };
        }
        ObjectMapper.Map(context, userNFTProtocolInfoIndex_to);
        await _userNFTProtocolIndexRepository.AddOrUpdateAsync(userNFTProtocolInfoIndex_to);

    }

    private async Task<NFTProtocolInfoIndex> GetNFTProtocolInfoIndex(Transferred eventValue,LogEventContext context)
    {
        var id = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol);
        var nftProtocolInfoIndex = await _nftProtocolInfoRepository.GetFromBlockStateSetAsync(id, context.ChainId); 
        return nftProtocolInfoIndex;
    }
    
    private async Task<NFTInfoIndex> GetNFTInfoIndex(Transferred eventValue,LogEventContext context)
    {
        var id = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol, eventValue.TokenId);
        var nftInfoIndex = await _nftInfoIndexRepository.GetFromBlockStateSetAsync(id, context.ChainId);
        return nftInfoIndex;
    }
    
}