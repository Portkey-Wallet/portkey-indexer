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

public class TokenTransferredLogEventProcessor:  CAHolderTokenBalanceProcessorBase<Transferred>
{
    public TokenTransferredLogEventProcessor(ILogger<TokenTransferredLogEventProcessor> logger,
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
        IObjectMapper objectMapper,IOptionsSnapshot<InscriptionListOptions> inscriptionListOptions,
        IAElfIndexerClientEntityRepository<CAHolderManagerIndex, TransactionInfo> caHolderManagerIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo> caHolderTransactionIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo> caHolderTransactionAddressIndexRepository,
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions = null) : base(logger, contractInfoOptions,
        caHolderIndexRepository, tokenInfoIndexRepository,nftCollectionInfoRepository,nftInfoRepository, caHolderSearchTokenNFTRepository,
        caHolderTokenBalanceIndexRepository,caHolderNFTCollectionBalanceIndexRepository, caHolderNFTBalanceIndexRepository, aelfDataProvider,
        objectMapper,inscriptionListOptions,
        caHolderManagerIndexRepository,
        caHolderTransactionIndexRepository,
        caHolderTransactionAddressIndexRepository,
        caHolderTransactionInfoOptions)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).TokenContractAddress;
    }

    protected override async Task HandleEventAsync(Transferred eventValue, LogEventContext context)
    {
        await HandlerTransactionIndexAsync(eventValue, context);
        var from = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.From.ToBase58()),context.ChainId);
        if (from != null)
        {
            await ModifyBalanceAsync(from.CAAddress, eventValue.Symbol, -eventValue.Amount, context);
        }
        var to = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.To.ToBase58()),context.ChainId);
        if (to == null) return;
        await ModifyBalanceAsync(to.CAAddress, eventValue.Symbol, eventValue.Amount, context);
    }

    protected override async Task HandlerTransactionIndexAsync(Transferred eventValue, LogEventContext context)
    {
        if (!IsValidTransaction(context.ChainId, context.To, context.MethodName, context.Params)) return;

        var from = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.From.ToBase58()),context.ChainId);
        var tokenInfoIndex = await GetTokenInfoIndexFromStateOrChainAsync(eventValue.Symbol, context);
        var nftInfoIndex = await GetNftInfoIndexFromStateOrChainAsync(eventValue.Symbol, context);
        if (from != null)
        {
            await AddCAHolderTransactionAddressAsync(from.CAAddress, eventValue.To.ToBase58(), context.ChainId,
                context);
            await CAHolderTransactionIndexRepository.AddOrUpdateAsync(GetCaHolderTransactionIndex(eventValue, tokenInfoIndex,nftInfoIndex,
                context));
        }

        var to = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.To.ToBase58()),context.ChainId);
        if (to == null) return;
        await AddCAHolderTransactionAddressAsync(to.CAAddress, eventValue.From.ToBase58(), context.ChainId, context);
        if (from != null) return;
        await CAHolderTransactionIndexRepository.AddOrUpdateAsync(GetCaHolderTransactionIndex(eventValue,
            tokenInfoIndex,nftInfoIndex, context));
    }
    
    private CAHolderTransactionIndex GetCaHolderTransactionIndex(Transferred transferred, TokenInfoIndex tokenInfoIndex, 
        NFTInfoIndex nftInfoIndex, LogEventContext context)
    {
        var id = IsMultiTransaction(context.ChainId, context.To, context.MethodName)
            ? IdGenerateHelper.GetId(context.BlockHash, context.TransactionId, transferred.To.ToBase58()) :
            IdGenerateHelper.GetId(context.BlockHash, context.TransactionId);
        var index = new CAHolderTransactionIndex
        {
            Id = id,
            Timestamp = context.BlockTime.ToTimestamp().Seconds,
            FromAddress = context.From,
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