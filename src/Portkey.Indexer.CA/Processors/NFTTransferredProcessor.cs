using AElf.Contracts.NFT;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class NFTTransferredProcessor : CAHolderTransactionProcessorBase<Transferred>
{

    public NFTTransferredProcessor(ILogger<NFTTransferredProcessor> logger,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> caHolderIndexRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, LogEventInfo> nftInfoIndexRepository,
        IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>
            caHolderTransactionIndexRepository,
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions, 
        IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo> caHolderTransactionAddressIndexRepository) :
        base(logger, caHolderIndexRepository, caHolderTransactionIndexRepository, tokenInfoIndexRepository,
            nftInfoIndexRepository,caHolderTransactionAddressIndexRepository,
            contractInfoOptions, caHolderTransactionInfoOptions, objectMapper)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).NFTContractAddress;
    }

    protected override async Task HandleEventAsync(Transferred eventValue, LogEventContext context)
    {
        if (!IsValidTransaction(context.ChainId, context.To, context.MethodName, context.Params)) return;

        var from = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.From.ToBase58()),context.ChainId);
        var nftInfoIndex =
            await NFTInfoIndexRepository.GetFromBlockStateSetAsync(
                IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol, eventValue.TokenId), context.ChainId);
        
        if (from != null)
        {
            await AddCAHolderTransactionAddressAsync(from.CAAddress, eventValue.To.ToBase58(), context.ChainId,context);
            await CAHolderTransactionIndexRepository.AddOrUpdateAsync(GetCaHolderTransactionIndex(eventValue, nftInfoIndex,
                context));
        }

        var to = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.To.ToBase58()),context.ChainId);
        if (to == null) return;
        await AddCAHolderTransactionAddressAsync(to.CAAddress, eventValue.From.ToBase58(), context.ChainId, context);
        if (from != null) return;
        await CAHolderTransactionIndexRepository.AddOrUpdateAsync(GetCaHolderTransactionIndex(eventValue,
                nftInfoIndex,
                context));
    }
    
    private CAHolderTransactionIndex GetCaHolderTransactionIndex(Transferred transferred, NFTInfoIndex nftInfoIndex, LogEventContext context)
    {
        var index = new CAHolderTransactionIndex
        {
            Id = IdGenerateHelper.GetId(context.BlockHash, context.TransactionId),
            Timestamp = context.BlockTime.ToTimestamp().Seconds,
            FromAddress = context.From,
            TokenInfo = new Entities.TokenInfo
            {
                Decimals = 0,
                Symbol = transferred.Symbol
            },
            NFTInfo = new Entities.NFTInfo
            {
                Alias = nftInfoIndex.Alias,
                Url = nftInfoIndex.ImageUrl,
                NFTId = transferred.TokenId
            },
            TransactionFee = GetTransactionFee(context.ExtraProperties),
            TransferInfo = new TransferInfo
            {
                Amount = transferred.Amount,
                FromAddress = transferred.From.ToBase58(),
                ToAddress = transferred.To.ToBase58(),
                FromChainId = context.ChainId,
                ToChainId = context.ChainId
            }
        };
        ObjectMapper.Map(context, index);
        index.MethodName = GetMethodName(context.MethodName, context.Params);
        return index;
    }
}