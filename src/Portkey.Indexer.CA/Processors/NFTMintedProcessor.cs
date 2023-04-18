using AElf.Contracts.NFT;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class NFTMintedProcessor :  CAHolderNFTBalanceProcessorBase<NFTMinted>
{
    private readonly IAElfIndexerClientEntityRepository<UserNFTInfoIndex, LogEventInfo> _userNFTInfoIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<NFTInfoIndex, LogEventInfo> _nftInfoIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<UserNFTProtocolInfoIndex, LogEventInfo> _userNFTProtocolIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<NFTProtocolInfoIndex, LogEventInfo> _nftProtocolIndexRepository;

    public NFTMintedProcessor(ILogger<NFTMintedProcessor> logger,
        IAElfIndexerClientEntityRepository<UserNFTInfoIndex, LogEventInfo> userNFTInfoIndexRepository,
        IAElfIndexerClientEntityRepository<UserNFTProtocolInfoIndex, LogEventInfo> userNFTProtocolIndexRepository,
        IAElfIndexerClientEntityRepository<NFTProtocolInfoIndex, LogEventInfo> nftProtocolIndexRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, LogEventInfo> nftInfoIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderSearchTokenNFTIndex, LogEventInfo> caHolderSearchTokenNFTRepository,
        IObjectMapper objectMapper, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions)
        : base(logger, contractInfoOptions, caHolderIndexRepository, nftInfoIndexRepository,
            caHolderSearchTokenNFTRepository, objectMapper)
    {
        _userNFTInfoIndexRepository = userNFTInfoIndexRepository;
        _userNFTProtocolIndexRepository = userNFTProtocolIndexRepository;
        _nftProtocolIndexRepository = nftProtocolIndexRepository;
        _nftInfoIndexRepository = nftInfoIndexRepository;
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).NFTContractAddress;
    }

    protected override async Task HandleEventAsync(NFTMinted eventValue, LogEventContext context)
    {
        var id = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol, eventValue.TokenId);
        var nftInfoIndex = await _nftInfoIndexRepository.GetFromBlockStateSetAsync(id,context.ChainId);
        if (nftInfoIndex != null)
        {
            nftInfoIndex.Quantity += eventValue.Quantity;
            nftInfoIndex.BlockHash = context.BlockHash;
            nftInfoIndex.BlockHeight = context.BlockHeight;
            nftInfoIndex.PreviousBlockHash = context.PreviousBlockHash;
        }
        else
        {
            nftInfoIndex = ObjectMapper.Map<NFTMinted, NFTInfoIndex>(eventValue);
            nftInfoIndex.Id = id;
            nftInfoIndex.NftContractAddress = GetContractAddress(context.ChainId);
            nftInfoIndex.Creator = eventValue.Creator.ToBase58();
            nftInfoIndex.Minter = eventValue.Minter.ToBase58();
            nftInfoIndex.Owner = eventValue.Owner.ToBase58();
            nftInfoIndex.TokenHash = eventValue.TokenHash.ToHex();
            nftInfoIndex.ImageUrl = 
                eventValue.Metadata.Value?.TryGetValue("ImageUrl", out var imageUrl)??false ? imageUrl : null;
            ObjectMapper.Map(context, nftInfoIndex);
        }

        await _nftInfoIndexRepository.AddOrUpdateAsync(nftInfoIndex);

        await UpdateUserNFTInfo(eventValue, context);

        await ModifyBalanceAsync(eventValue.Owner.ToBase58(), eventValue.Symbol, eventValue.TokenId,
            eventValue.Quantity, context);
    }

    private async Task UpdateUserNFTInfo(NFTMinted eventValue, LogEventContext context)
    {
        //check NFT info index is exist
        var nftInfoId = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol, eventValue.TokenId);
        var nftInfoIndex = await _nftInfoIndexRepository.GetFromBlockStateSetAsync(nftInfoId, context.ChainId);
        if (nftInfoIndex == null) return;
        
        //Update userNFTInfoIndex
        var infoIndexId = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol, eventValue.TokenId,
            eventValue.Owner.ToBase58());
        var userNFTInfoIndex = await _userNFTInfoIndexRepository.GetFromBlockStateSetAsync(infoIndexId, context.ChainId);
        if (userNFTInfoIndex != null)
        {
            userNFTInfoIndex.Balance += eventValue.Quantity;
        }
        else
        {
            var caHolder =
                await CAHolderIndexRepository.GetFromBlockStateSetAsync(
                    IdGenerateHelper.GetId(context.ChainId, eventValue.Owner.ToBase58()), context.ChainId);
            if (caHolder == null) return;
            userNFTInfoIndex = new UserNFTInfoIndex()
            {
                Id = infoIndexId,
                CAAddress = eventValue.Owner.ToBase58(),
                Balance = eventValue.Quantity,
                NftInfo = ObjectMapper.Map<NFTInfoIndex, NFTItemInfo>(nftInfoIndex)
            };
        }
        ObjectMapper.Map(context, userNFTInfoIndex);
        await _userNFTInfoIndexRepository.AddOrUpdateAsync(userNFTInfoIndex);
        
        //Update nftInfoIndex
        // nftInfoIndex.Quantity += eventValue.Quantity;
        // _objectMapper.Map(context, nftInfoIndex);
        // await _nftInfoIndexRepository.AddOrUpdateAsync(nftInfoIndex);

        //check NFT protocol index is exist
        var nftProtocolId = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol);
        var nftProtocolInfoIndex =
            await _nftProtocolIndexRepository.GetFromBlockStateSetAsync(nftProtocolId, context.ChainId);

        //Update userNFTProtocolInfoIndex
        var protocolIndexId =
            IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol, eventValue.Owner.ToBase58());
        var userNFTProtocolInfoIndex = await _userNFTProtocolIndexRepository.GetFromBlockStateSetAsync(protocolIndexId, context.ChainId);
        if (userNFTProtocolInfoIndex != null)
        {
            if (!userNFTProtocolInfoIndex.TokenIds.Contains(eventValue.TokenId))
            {
                userNFTProtocolInfoIndex.TokenIds.Add(eventValue.TokenId);
            }
        }
        else
        {
            userNFTProtocolInfoIndex = new UserNFTProtocolInfoIndex()
            {
                Id = protocolIndexId,
                CAAddress = eventValue.Owner.ToBase58(),
                TokenIds = new List<long>() { eventValue.TokenId },
                NftProtocolInfo = ObjectMapper.Map<NFTProtocolInfoIndex, NFTProtocol>(nftProtocolInfoIndex)
            };
        }
        ObjectMapper.Map(context, userNFTProtocolInfoIndex);
        await _userNFTProtocolIndexRepository.AddOrUpdateAsync(userNFTProtocolInfoIndex);
        
        //Update NFTProtocolInfoIndex
        nftProtocolInfoIndex.Supply += eventValue.Quantity;
        ObjectMapper.Map(context, nftProtocolInfoIndex);
        await _nftProtocolIndexRepository.AddOrUpdateAsync(nftProtocolInfoIndex);
    }
}