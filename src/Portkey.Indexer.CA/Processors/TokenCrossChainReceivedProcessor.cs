using AElf;
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

public class TokenCrossChainReceivedProcessor : CAHolderTransactionProcessorBase<CrossChainReceived>
{
    private readonly ILogger<TokenCrossChainReceivedProcessor> _logger;
    public TokenCrossChainReceivedProcessor(ILogger<TokenCrossChainReceivedProcessor> logger,
        IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderManagerIndex, TransactionInfo> caHolderManagerIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>
            caHolderTransactionIndexRepository,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, TransactionInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, TransactionInfo> nftInfoIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex,TransactionInfo> caHolderTransactionAddressIndexRepository
,        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions, IObjectMapper objectMapper) :
        base(logger, caHolderIndexRepository,caHolderManagerIndexRepository, caHolderTransactionIndexRepository, tokenInfoIndexRepository,
            nftInfoIndexRepository,caHolderTransactionAddressIndexRepository, contractInfoOptions, caHolderTransactionInfoOptions, objectMapper)
    {
        _logger = logger;
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).TokenContractAddress;
    }

    protected override async Task HandleEventAsync(CrossChainReceived eventValue, LogEventContext context)
    {
        _logger.LogInformation("[TokenCrossChainReceivedProcessor] in TokenCrossChainReceivedProcessor, eventValue: {eventValue}",
            JsonConvert.SerializeObject(eventValue));
        
        if (!IsValidTransaction(context.ChainId, context.To, context.MethodName, context.Params)) return;
        
        _logger.LogInformation("[TokenCrossChainReceivedProcessor] before tokenInfoIndex, chainId:{chainId}, symbol:{symbol}, txid:{txid}",
            context.ChainId,
            eventValue.Symbol, context.TransactionId);
        var tokenInfoIndex =
            await TokenInfoIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol),context.ChainId);
        
        _logger.LogInformation("[TokenCrossChainReceivedProcessor] before nftInfoIndex, chainId:{chainId}, symbol:{symbol}, txid:{txid}", context.ChainId,
            eventValue.Symbol, context.TransactionId);
        var nftInfoIndex =
            await NFTInfoIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol),context.ChainId);
        
        _logger.LogInformation("[TokenCrossChainReceivedProcessor] before from_manager, chainId:{chainId}, from:{from}, txid:{txid}", context.ChainId,
            eventValue.From.ToBase58(), context.TransactionId);
        var from_manager = await CAHolderManagerIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.From.ToBase58()),context.ChainId);
        
        string fromManagerCAAddress = from_manager == null ? "" : from_manager.CAAddresses.FirstOrDefault();
        await CAHolderTransactionIndexRepository.AddOrUpdateAsync(GetCaHolderTransactionIndex(eventValue, tokenInfoIndex,nftInfoIndex,
            fromManagerCAAddress,context));
        
        _logger.LogInformation("[TokenCrossChainReceivedProcessor] before to_ca, chainId:{chainId}, to:{to}, txid:{txid}", context.ChainId,
            eventValue.To.ToBase58(), context.TransactionId);
        
        var to_ca = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.To.ToBase58()),context.ChainId);
        if (to_ca != null)
        {
            await AddCAHolderTransactionAddressAsync(to_ca.CAAddress, eventValue.From.ToBase58(),
                ChainHelper.ConvertChainIdToBase58(eventValue.FromChainId), context);
        }
        else
        {
            _logger.LogInformation("[TokenCrossChainReceivedProcessor] before to_manager, chainId:{chainId}, to:{to}, txid:{txid}", context.ChainId,
                eventValue.To.ToBase58(), context.TransactionId);
            var to_manager = await CAHolderManagerIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
                eventValue.To.ToBase58()),context.ChainId);
            if (to_manager != null)
            {
                await AddCAHolderTransactionAddressAsync(to_manager.CAAddresses.FirstOrDefault(), eventValue.From.ToBase58(),
                    ChainHelper.ConvertChainIdToBase58(eventValue.FromChainId), context);
            }
        }
        
    }
    
    private CAHolderTransactionIndex GetCaHolderTransactionIndex(CrossChainReceived transferred, TokenInfoIndex tokenInfoIndex,
        NFTInfoIndex nftInfoIndex,string fromManagerCAAddress, LogEventContext context)
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
            TokenInfo=tokenInfoIndex,
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
                TransferTransactionId = transferred.TransferTransactionId.ToHex()
            }
        };
        ObjectMapper.Map(context, index);
        index.MethodName = GetMethodName(context.MethodName, context.Params);
        return index;
    }
}