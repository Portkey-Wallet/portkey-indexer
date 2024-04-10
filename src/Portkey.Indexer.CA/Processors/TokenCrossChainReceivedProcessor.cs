using AElf;
using AElf.Contracts.MultiToken;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Portkey.Indexer.CA.Provider;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class TokenCrossChainReceivedProcessor:  CAHolderTokenBalanceProcessorBase<CrossChainReceived>
{
    public TokenCrossChainReceivedProcessor(ILogger<TokenCrossChainReceivedProcessor> logger,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, TransactionInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTCollectionInfoIndex, TransactionInfo> nftCollectionInfoRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, TransactionInfo> nftInfoRepository,
        IAElfIndexerClientEntityRepository<CAHolderSearchTokenNFTIndex, TransactionInfo> caHolderSearchTokenNFTRepository,
        IAElfIndexerClientEntityRepository<CAHolderTokenBalanceIndex, TransactionInfo>
            caHolderTokenBalanceIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderNFTCollectionBalanceIndex, TransactionInfo> caHolderNFTCollectionBalanceIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderNFTBalanceIndex, TransactionInfo> caHolderNFTBalanceIndexRepository,
        IAElfDataProvider aelfDataProvider,
        IObjectMapper objectMapper,
        IOptionsSnapshot<InscriptionListOptions> inscriptionListOptions
        ) : base(logger, contractInfoOptions,
        caHolderIndexRepository, tokenInfoIndexRepository,nftCollectionInfoRepository,nftInfoRepository, caHolderSearchTokenNFTRepository,
        caHolderTokenBalanceIndexRepository,caHolderNFTCollectionBalanceIndexRepository, caHolderNFTBalanceIndexRepository, aelfDataProvider,
        objectMapper,inscriptionListOptions)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).TokenContractAddress;
    }

    protected override async Task HandleEventAsync(CrossChainReceived eventValue, LogEventContext context)
    {
        await HandlerTransactionIndexAsync(eventValue, context);
        var holder = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.To.ToBase58()),context.ChainId);
        if (holder == null) return;
        await ModifyBalanceAsync(holder.CAAddress, eventValue.Symbol, eventValue.Amount, context);
    }
    
    protected override async Task HandlerTransactionIndexAsync(CrossChainReceived eventValue, LogEventContext context)
    {
        if (!IsValidTransaction(context.ChainId, context.To, context.MethodName, context.Params)) return;
        
        var tokenInfoIndex = await GetTokenInfoIndexFromStateOrChainAsync(eventValue.Symbol, context);
        var nftInfoIndex = await GetNftInfoIndexFromStateOrChainAsync(eventValue.Symbol, context);
        var from_manager = await CAHolderManagerIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.From.ToBase58()),context.ChainId);
        string fromManagerCAAddress = from_manager == null ? "" : from_manager.CAAddresses.FirstOrDefault();
        await CAHolderTransactionIndexRepository.AddOrUpdateAsync(GetCaHolderTransactionIndex(eventValue, tokenInfoIndex,nftInfoIndex,
            fromManagerCAAddress,context));
        
        var to_ca = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.To.ToBase58()),context.ChainId);
        if (to_ca != null)
        {
            await AddCAHolderTransactionAddressAsync(to_ca.CAAddress, eventValue.From.ToBase58(),
                ChainHelper.ConvertChainIdToBase58(eventValue.FromChainId), context);
        }
        else
        {
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