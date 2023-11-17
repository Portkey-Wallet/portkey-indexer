using AElf.Contracts.MultiToken;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class TokenTransferredProcessor : CAHolderTransactionProcessorBase1<Transferred>
{
    private readonly ILogger<TokenTransferredProcessor> _logger;

    public TokenTransferredProcessor(ILogger<TokenTransferredProcessor> logger,
        IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderManagerIndex, TransactionInfo> caHolderManagerIndexRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, TransactionInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, TransactionInfo> nftInfoIndexRepository,
        IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>
            caHolderTransactionIndexRepository,
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions,
        IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo>
            caHolderTransactionAddressIndexRepository) :
        base(logger, caHolderIndexRepository, caHolderManagerIndexRepository, caHolderTransactionIndexRepository,
            tokenInfoIndexRepository,
            nftInfoIndexRepository, caHolderTransactionAddressIndexRepository,
            contractInfoOptions, caHolderTransactionInfoOptions, objectMapper)
    {
        _logger = logger;
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).TokenContractAddress;
    }

    protected override async Task HandleEventAsync(Transferred eventValue, LogEventContext context)
    {
        _logger.LogInformation("[TokenTransferredProcessor] in TokenTransferredProcessor, eventValue: {eventValue}",
            JsonConvert.SerializeObject(eventValue));

        if (!IsValidTransaction(context.ChainId, context.To, context.MethodName, context.Params)) return;

        _logger.LogInformation("[TokenTransferredProcessor] before from, chainId:{chainId}, from:{from}, txid:{txid}", context.ChainId,
            eventValue.From.ToBase58(), context.TransactionId);
        var from = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.From.ToBase58()), context.ChainId);

        _logger.LogInformation("[TokenTransferredProcessor] before tokenInfoIndex, chainId:{chainId}, symbol:{symbol}, txid:{txid}",
            context.ChainId,
            eventValue.Symbol, context.TransactionId);
        var tokenInfoIndex =
            await TokenInfoIndexRepository.GetFromBlockStateSetAsync(
                IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol), context.ChainId);

        _logger.LogInformation("[TokenTransferredProcessor] before nftInfoIndex, chainId:{chainId}, symbol:{symbol}, txid:{txid}", context.ChainId,
            eventValue.Symbol, context.TransactionId);
        var nftInfoIndex =
            await NFTInfoIndexRepository.GetFromBlockStateSetAsync(
                IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol), context.ChainId);
        if (from != null)
        {
            await AddCAHolderTransactionAddressAsync(from.CAAddress, eventValue.To.ToBase58(), context.ChainId,
                context);
            await CAHolderTransactionIndexRepository.AddOrUpdateAsync(GetCaHolderTransactionIndex(eventValue,
                tokenInfoIndex, nftInfoIndex,
                context));
        }

        _logger.LogInformation("[TokenTransferredProcessor] before to, chainId:{chainId}, to:{to}, txid:{txid}", context.ChainId,
            eventValue.To.ToBase58(), context.TransactionId);
        var to = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.To.ToBase58()), context.ChainId);
        
        if (to == null) return;
        await AddCAHolderTransactionAddressAsync(to.CAAddress, eventValue.From.ToBase58(), context.ChainId, context);
        if (from != null) return;
        await CAHolderTransactionIndexRepository.AddOrUpdateAsync(GetCaHolderTransactionIndex(eventValue,
            tokenInfoIndex, nftInfoIndex, context));
    }

    private CAHolderTransactionIndex GetCaHolderTransactionIndex(Transferred transferred, TokenInfoIndex tokenInfoIndex,
        NFTInfoIndex nftInfoIndex, LogEventContext context)
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
                FromCAAddress = transferred.From.ToBase58(),
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