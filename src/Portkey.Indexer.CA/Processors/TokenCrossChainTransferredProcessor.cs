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

public class TokenCrossChainTransferredProcessor : CAHolderTransactionProcessorBase<CrossChainTransferred>
{
    private readonly IAElfIndexerClientEntityRepository<CompatibleCrossChainTransferIndex, TransactionInfo>
        _compatibleCrossChainTransferIndexRepository;

    public TokenCrossChainTransferredProcessor(ILogger<TokenCrossChainTransferredProcessor> logger,
        IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderManagerIndex, TransactionInfo> caHolderManagerIndexRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, TransactionInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, TransactionInfo> nftInfoIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo>
            caHolderTransactionAddressIndexRepository,
        IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>
            caHolderTransactionIndexRepository,
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions,
        IAElfDataProvider aelfDataProvider,
        IAElfIndexerClientEntityRepository<CompatibleCrossChainTransferIndex, TransactionInfo>
            compatibleCrossChainTransferIndexRepository) :
        base(logger, caHolderIndexRepository, caHolderManagerIndexRepository, caHolderTransactionIndexRepository,
            tokenInfoIndexRepository, nftInfoIndexRepository,
            caHolderTransactionAddressIndexRepository, contractInfoOptions, caHolderTransactionInfoOptions,
            objectMapper, aelfDataProvider)
    {
        _compatibleCrossChainTransferIndexRepository = compatibleCrossChainTransferIndexRepository;
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).TokenContractAddress;
    }

    protected override async Task HandleEventAsync(CrossChainTransferred eventValue, LogEventContext context)
    {
        if (!IsValidTransaction(context.ChainId, context.To, context.MethodName, context.Params)) return;

        var toCaHolder = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.To.ToBase58()), context.ChainId);
        var fromManager = await CAHolderManagerIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(
            context.ChainId,
            eventValue.From.ToBase58()), context.ChainId);

        if (fromManager == null && toCaHolder == null)
        {
            return;
        }

        var tokenInfoIndex = await GetTokenInfoIndexFromStateOrChainAsync(eventValue.Symbol, context);
        var nftInfoIndex = await GetNftInfoIndexFromStateOrChainAsync(eventValue.Symbol, context);

        var fromManagerCaAddress = string.Empty;
        if (fromManager != null)
        {
            fromManagerCaAddress = fromManager.CAAddresses.FirstOrDefault();
            await AddCAHolderTransactionAddressAsync(fromManager.CAAddresses.FirstOrDefault(), eventValue.To.ToBase58(),
                ChainHelper.ConvertChainIdToBase58(eventValue.ToChainId), context);
        }
        else
        {
            await AddCompatibleCrossChainTransferAsync(context);
        }

        await CAHolderTransactionIndexRepository.AddOrUpdateAsync(GetCaHolderTransactionIndex(eventValue,
            tokenInfoIndex, nftInfoIndex,
            fromManagerCaAddress, context));
    }

    private CAHolderTransactionIndex GetCaHolderTransactionIndex(CrossChainTransferred transferred,
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
                FromChainId = context.ChainId,
                ToChainId = ChainHelper.ConvertChainIdToBase58(transferred.ToChainId)
            }
        };
        ObjectMapper.Map(context, index);
        index.MethodName = GetMethodName(context.MethodName, context.Params);
        return index;
    }

    private async Task AddCompatibleCrossChainTransferAsync(LogEventContext context)
    {
        var index = new CompatibleCrossChainTransferIndex
        {
            Id = IdGenerateHelper.GetId(context.BlockHash, context.TransactionId),
            Timestamp = context.BlockTime.ToTimestamp().Seconds,
            FromAddress = context.From,
            ToAddress = context.To
        };

        ObjectMapper.Map(context, index);

        await _compatibleCrossChainTransferIndexRepository.AddOrUpdateAsync(index);
    }
}