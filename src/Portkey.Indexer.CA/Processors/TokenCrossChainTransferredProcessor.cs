using AElf;
using AElf.Contracts.MultiToken;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class TokenCrossChainTransferredProcessor : CAHolderTransactionProcessorBase<CrossChainTransferred>
{
    public TokenCrossChainTransferredProcessor(ILogger<TokenCrossChainTransferredProcessor> logger,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderManagerIndex, LogEventInfo> caHolderManagerIndexRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, LogEventInfo> nftInfoIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex,TransactionInfo> caHolderTransactionAddressIndexRepository,
        IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>
            caHolderTransactionIndexRepository,
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions) :
        base(logger, caHolderIndexRepository,caHolderManagerIndexRepository, caHolderTransactionIndexRepository, tokenInfoIndexRepository,nftInfoIndexRepository,
            caHolderTransactionAddressIndexRepository,contractInfoOptions, caHolderTransactionInfoOptions, objectMapper)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).TokenContractAddress;
    }

    protected override async Task HandleEventAsync(CrossChainTransferred eventValue, LogEventContext context)
    {
        if (!IsValidTransaction(context.ChainId, context.To, context.MethodName, context.Params)) return;

        // var from = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
        //     eventValue.From.ToBase58()),context.ChainId);
        var from_manager = await CAHolderManagerIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
        eventValue.From.ToBase58()),context.ChainId);
        
        if (from_manager != null)
        {
            var tokenInfoIndex =
                await TokenInfoIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol),context.ChainId);
            var nftInfoIndex =
                await NFTInfoIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol),context.ChainId);
            string fromManagerCAAddress = from_manager.CAAddresses.FirstOrDefault();
            await CAHolderTransactionIndexRepository.AddOrUpdateAsync(GetCaHolderTransactionIndex(eventValue, tokenInfoIndex,nftInfoIndex,
                fromManagerCAAddress,context));
            await AddCAHolderTransactionAddressAsync(from_manager.CAAddresses.FirstOrDefault(), eventValue.To.ToBase58(),
                ChainHelper.ConvertChainIdToBase58(eventValue.ToChainId), context);
        }
    }
    
    private CAHolderTransactionIndex GetCaHolderTransactionIndex(CrossChainTransferred transferred, TokenInfoIndex tokenInfoIndex, 
        NFTInfoIndex nftInfoIndex,string fromManagerCAAddress,  LogEventContext context)
    {
        var index = new CAHolderTransactionIndex
        {
            Id = IdGenerateHelper.GetId(context.BlockHash, context.TransactionId),
            Timestamp = context.BlockTime.ToTimestamp().Seconds,
            FromAddress = context.From,
            // TokenInfo = new Entities.TokenInfo
            // {
            //     Decimals = tokenInfoIndex.Decimals,
            //     Symbol = tokenInfoIndex.Symbol
            // },
            TokenInfo = tokenInfoIndex,
            NftInfo = nftInfoIndex,
            TransactionFee = GetTransactionFee(context.ExtraProperties),
            TransferInfo = new TransferInfo
            {
                Amount = transferred.Amount,
                FromAddress = transferred.From.ToBase58(),
                FromCAAddress = fromManagerCAAddress,
                ToAddress = transferred.To.ToBase58(),
                FromChainId = context.ChainId,
                ToChainId = ChainHelper.ConvertChainIdToBase58(transferred.ToChainId)
            }
        };
        ObjectMapper.Map(context, index);
        index.MethodName = GetMethodName(context.MethodName, context.Params);
        return index;
    }
}