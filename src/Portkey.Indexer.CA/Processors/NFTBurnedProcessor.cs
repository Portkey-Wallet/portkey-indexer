using AElf.Contracts.NFT;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class NFTBurnedProcessor :  CAHolderNFTBalanceProcessorBase<Burned>
{
    private readonly IAElfIndexerClientEntityRepository<UserNFTInfoIndex, LogEventInfo> _userNFTInfoIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<NFTInfoIndex, LogEventInfo> _nftInfoIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<UserNFTProtocolInfoIndex, LogEventInfo> _userNFTProtocolIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<NFTProtocolInfoIndex, LogEventInfo> _nftProtocolIndexRepository;

    public NFTBurnedProcessor(ILogger<NFTBurnedProcessor> logger,
        IAElfIndexerClientEntityRepository<UserNFTInfoIndex, LogEventInfo> userNFTInfoIndexRepository,
        IAElfIndexerClientEntityRepository<UserNFTProtocolInfoIndex, LogEventInfo> userNFTProtocolIndexRepository,
        IObjectMapper objectMapper, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<NFTProtocolInfoIndex, LogEventInfo> nftProtocolIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderSearchTokenNFTIndex, LogEventInfo> caHolderSearchTokenNFTRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, LogEventInfo> nftInfoIndexRepository) : base(logger,
        contractInfoOptions, caHolderIndexRepository, nftInfoIndexRepository, caHolderSearchTokenNFTRepository,
        objectMapper)
    {
        _userNFTInfoIndexRepository = userNFTInfoIndexRepository;
        _userNFTProtocolIndexRepository = userNFTProtocolIndexRepository;
        _nftProtocolIndexRepository = nftProtocolIndexRepository;
        _nftInfoIndexRepository = nftInfoIndexRepository;
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).NFTContractAddress;
    }

    protected override async Task HandleEventAsync(Burned eventValue, LogEventContext context)
    {
        //check NFT info index is exist
        var nftInfoId = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol, eventValue.TokenId);
        var nftInfoIndex = await _nftInfoIndexRepository.GetFromBlockStateSetAsync(nftInfoId, context.ChainId);
        if (nftInfoIndex == null) return;
        
        //Update userNFTInfoIndex
        var id = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol, eventValue.TokenId, eventValue.Burner.ToBase58());
        var userNFTInfoIndex = await _userNFTInfoIndexRepository.GetFromBlockStateSetAsync(id,context.ChainId);
        if (userNFTInfoIndex == null)
        {
            return;
        }
        ObjectMapper.Map(context, userNFTInfoIndex);
        userNFTInfoIndex.Balance -= eventValue.Amount;
        //TODO Quantity == 0, delete?
        await _userNFTInfoIndexRepository.AddOrUpdateAsync(userNFTInfoIndex);
        
        //Update nftInfoIndex
        nftInfoIndex.Quantity -= eventValue.Amount;
        ObjectMapper.Map(context, nftInfoIndex);
        await _nftInfoIndexRepository.AddOrUpdateAsync(nftInfoIndex);

        //check NFT protocol index is exist
        var nftProtocolId = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol);
        var nftProtocolInfoIndex =
            await _nftProtocolIndexRepository.GetFromBlockStateSetAsync(nftProtocolId, context.ChainId);
        if (nftProtocolInfoIndex == null) return;
        
        //Update userNFTProtocolInfoIndex
        var protocolIndexId =
            IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol, eventValue.Burner.ToBase58());
        var userNFTProtocolInfoIndex = await _userNFTProtocolIndexRepository.GetFromBlockStateSetAsync(protocolIndexId,context.ChainId);
        if (userNFTProtocolInfoIndex != null)
        {
            ObjectMapper.Map(context, userNFTProtocolInfoIndex);
            //TODO check Quantity == 0 ,remove TokenIds
            if (userNFTInfoIndex.Balance == 0 && userNFTProtocolInfoIndex.TokenIds.Contains(eventValue.TokenId))
            {
                userNFTProtocolInfoIndex.TokenIds.Remove(eventValue.TokenId);
            }
        }
        await _userNFTProtocolIndexRepository.AddOrUpdateAsync(userNFTProtocolInfoIndex);
        
        //Update nftProtocolInfoIndex
        nftProtocolInfoIndex.Supply -= eventValue.Amount;
        ObjectMapper.Map(context, nftProtocolInfoIndex);
        await _nftProtocolIndexRepository.AddOrUpdateAsync(nftProtocolInfoIndex);
        
        await ModifyBalanceAsync(eventValue.Burner.ToBase58(), eventValue.Symbol, eventValue.TokenId,
            -eventValue.Amount, context);
    }
}