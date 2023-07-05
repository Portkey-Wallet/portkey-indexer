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

public class TokenCrossChainReceivedProcessor : CAHolderTransactionProcessorBase<CrossChainReceived>
{
    public TokenCrossChainReceivedProcessor(ILogger<TokenCrossChainReceivedProcessor> logger,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderManagerIndex, LogEventInfo> caHolderManagerIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>
            caHolderTransactionIndexRepository,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, LogEventInfo> nftInfoIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo>
            caHolderTransactionAddressIndexRepository
        , IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions, IObjectMapper objectMapper) :
        base(logger, caHolderIndexRepository, caHolderManagerIndexRepository, caHolderTransactionIndexRepository,
            tokenInfoIndexRepository,
            nftInfoIndexRepository, caHolderTransactionAddressIndexRepository, contractInfoOptions,
            caHolderTransactionInfoOptions, objectMapper)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).TokenContractAddress;
    }

    protected override async Task HandleEventAsync(CrossChainReceived eventValue, LogEventContext context)
    {
        if (!IsValidTransaction(context.ChainId, context.To, context.MethodName, context.Params)) return;

        var tokenInfoIndex =
            await TokenInfoIndexRepository.GetFromBlockStateSetAsync(
                IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol), context.ChainId);
        var nftInfoIndex =
            await NFTInfoIndexRepository.GetFromBlockStateSetAsync(
                IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol), context.ChainId);
        var from_manager = await CAHolderManagerIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(
            context.ChainId,
            eventValue.From.ToBase58()), context.ChainId);
        string fromManagerCAAddress = from_manager == null ? "" : from_manager.CAAddresses.FirstOrDefault();
        await CAHolderTransactionIndexRepository.AddOrUpdateAsync(GetCaHolderTransactionIndex(eventValue,
            tokenInfoIndex, nftInfoIndex,
            fromManagerCAAddress, context));

        var to_ca = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.To.ToBase58()), context.ChainId);
        if (to_ca != null)
        {
            await AddCAHolderTransactionAddressAsync(to_ca.CAAddress, eventValue.From.ToBase58(),
                ChainHelper.ConvertChainIdToBase58(eventValue.FromChainId), context);
        }
        else
        {
            var to_manager = await CAHolderManagerIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(
                context.ChainId,
                eventValue.To.ToBase58()), context.ChainId);
            if (to_manager != null)
            {
                await AddCAHolderTransactionAddressAsync(to_manager.CAAddresses.FirstOrDefault(),
                    eventValue.From.ToBase58(),
                    ChainHelper.ConvertChainIdToBase58(eventValue.FromChainId), context);
            }
        }
    }

    private CAHolderTransactionIndex GetCaHolderTransactionIndex(CrossChainReceived transferred,
        TokenInfoIndex tokenInfoIndex,
        NFTInfoIndex nftInfoIndex, string fromManagerCAAddress, LogEventContext context)
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
                FromChainId = ChainHelper.ConvertChainIdToBase58(transferred.FromChainId),
                ToChainId = context.ChainId,
                TransferTransactionId = transferred.TransferTransactionId.ToHex(),
                Memo = transferred.Memo
            }
        };
        ObjectMapper.Map(context, index);
        index.MethodName = GetMethodName(context.MethodName, context.Params);
        return index;
    }
}