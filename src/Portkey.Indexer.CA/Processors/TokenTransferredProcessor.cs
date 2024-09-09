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

public class TokenTransferredProcessor:  CAHolderTokenBalanceProcessorBase<Transferred>
{
    public TokenTransferredProcessor(ILogger<TokenTransferredProcessor> logger,
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
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions) : base(logger, contractInfoOptions,
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
        else
        {
            await ModifyBalanceAsync(eventValue.From.ToBase58(), eventValue.Symbol, -eventValue.Amount, context);
        }
        var to = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.To.ToBase58()),context.ChainId);
        if (to != null)
        {
            await ModifyBalanceAsync(to.CAAddress, eventValue.Symbol, eventValue.Amount, context);
        }
        else
        {
            await ModifyBalanceAsync(eventValue.To.ToBase58(), eventValue.Symbol, eventValue.Amount, context);
        }
    }

    protected override async Task HandlerTransactionIndexAsync(Transferred eventValue, LogEventContext context)
    {
        var from = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.From.ToBase58()),context.ChainId);
        var tokenInfoIndex = await GetTokenInfoIndexFromStateOrChainAsync(eventValue.Symbol, context);
        var nftInfoIndex = await GetNftInfoIndexFromStateOrChainAsync(eventValue.Symbol, context);
        if (from != null)
        {
            await AddCAHolderTransactionAddressAsync(from.CAAddress, eventValue.To.ToBase58(), context.ChainId,
                context);
            await CAHolderTransactionIndexRepository.AddOrUpdateAsync(await GetCaHolderTransactionIndexAsync(eventValue, tokenInfoIndex,nftInfoIndex,
                context));
        }

        var to = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.To.ToBase58()),context.ChainId);
        if (to == null) return;
        await AddCAHolderTransactionAddressAsync(to.CAAddress, eventValue.From.ToBase58(), context.ChainId, context);
        if (from != null) return;
        await CAHolderTransactionIndexRepository.AddOrUpdateAsync(await GetCaHolderTransactionIndexAsync(eventValue,
            tokenInfoIndex,nftInfoIndex, context));
    }
    
    private async Task<CAHolderTransactionIndex> GetCaHolderTransactionIndexAsync(Transferred transferred, TokenInfoIndex tokenInfoIndex, 
        NFTInfoIndex nftInfoIndex, LogEventContext context)
    {
        var id = IsMultiTransaction(context.ChainId, context.To, context.MethodName)
            ? IdGenerateHelper.GetId(context.BlockHash, context.TransactionId, transferred.To.ToBase58()) :
            IdGenerateHelper.GetId(context.BlockHash, context.TransactionId);
        var index = await CAHolderTransactionIndexRepository.GetFromBlockStateSetAsync(id, context.ChainId);
        index ??= new CAHolderTransactionIndex
        {
            Id = id,
            Timestamp = context.BlockTime.ToTimestamp().Seconds,
            FromAddress = context.From,
            TransactionFee = GetTransactionFee(context.ExtraProperties),
            ToContractAddress = GetToContractAddress(context.ChainId, context.To, context.MethodName, context.Params)
        };
        if (index.TransferInfo != null)
        {
            index.TokenTransferInfos.Add(new TokenTransferInfo
            {
                TransferInfo = index.TransferInfo,
                TokenInfo = index.TokenInfo,
                NftInfo = index.NftInfo
            });
            index.TransferInfo = null;
            index.NftInfo = null;
            index.TokenInfo = null;
        }

        var transferInfo = new TransferInfo
        {
            Amount = transferred.Amount,
            FromAddress = transferred.From.ToBase58(),
            FromCAAddress = transferred.From.ToBase58(),
            ToAddress = transferred.To.ToBase58(),
            FromChainId = context.ChainId,
            ToChainId = context.ChainId
        };
        if (index.TokenTransferInfos.Count > 0)
        {
            index.TokenTransferInfos.Add(new TokenTransferInfo
            {
                TransferInfo = transferInfo,
                TokenInfo = tokenInfoIndex,
                NftInfo = nftInfoIndex
            });
        }
        else
        {
            index.TransferInfo = transferInfo;
            index.TokenInfo = tokenInfoIndex;
            index.NftInfo = nftInfoIndex;
        }
        ObjectMapper.Map(context, index);
        index.MethodName = GetMethodName(context.MethodName, context.Params);
        return index;
    }
}