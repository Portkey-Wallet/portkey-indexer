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

public class TokenApprovedProcessor : CAHolderTransactionProcessorBase<Approved>
{
    private readonly IAElfIndexerClientEntityRepository<CAHolderTokenApprovedIndex, TransactionInfo> _batchApprovedIndexRepository;
    public TokenApprovedProcessor(ILogger<TokenApprovedProcessor> logger,
        IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderManagerIndex, TransactionInfo> caHolderManagerIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>
            caHolderTransactionIndexRepository,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, TransactionInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, TransactionInfo> nftInfoIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo>
            caHolderTransactionAddressIndexRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions,
        IAElfIndexerClientEntityRepository<CAHolderTokenApprovedIndex, TransactionInfo> batchApprovedIndexRepository,
        IObjectMapper objectMapper) :
        base(logger, caHolderIndexRepository, caHolderManagerIndexRepository, caHolderTransactionIndexRepository,
            tokenInfoIndexRepository,
            nftInfoIndexRepository, caHolderTransactionAddressIndexRepository, contractInfoOptions,
            caHolderTransactionInfoOptions, objectMapper)
    {
        _batchApprovedIndexRepository = batchApprovedIndexRepository;
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).TokenContractAddress;
    }

    protected override async Task HandleEventAsync(Approved eventValue, LogEventContext context)
    {
        await HandlerTransactionIndexAsync(eventValue, context);
        
        var holder = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.Owner.ToBase58()), context.ChainId);
        if (holder == null) return;
        var batchApprovedIndexId = IdGenerateHelper.GetId(context.ChainId, eventValue.Owner.ToBase58(), eventValue.Spender.ToBase58());
        var batchApprovedIndex =
            await _batchApprovedIndexRepository.GetFromBlockStateSetAsync(batchApprovedIndexId, context.ChainId);
        if (batchApprovedIndex == null)
        {
            batchApprovedIndex = new CAHolderTokenApprovedIndex
            {
                Id = batchApprovedIndexId,
                Spender = eventValue.Spender.ToBase58(),
                CAAddress = eventValue.Owner.ToBase58(),
            };
        }
        batchApprovedIndex.BatchApprovedAmount =
            CommonConstant.BatchApprovedSymbol.Equals(eventValue.Symbol) ? eventValue.Amount : 0;
        ObjectMapper.Map(context, batchApprovedIndex);
        await _batchApprovedIndexRepository.AddOrUpdateAsync(batchApprovedIndex);
    }

    protected override async Task HandlerTransactionIndexAsync(Approved eventValue, LogEventContext context)
    {
        if (!IsValidTransaction(context.ChainId, context.To, context.MethodName, context.Params)) return;
        var holder = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.Owner.ToBase58()), context.ChainId);
        if (holder == null) return;
        var index = new CAHolderTransactionIndex
        {
            Id = IdGenerateHelper.GetId(context.BlockHash, context.TransactionId),
            Timestamp = context.BlockTime.ToTimestamp().Seconds,
            FromAddress = eventValue.Owner.ToBase58(),
            TransactionFee = GetTransactionFee(context.ExtraProperties)
        };
        ObjectMapper.Map(context, index);
        index.MethodName = GetMethodName(context.MethodName, context.Params);
        await CAHolderTransactionIndexRepository.AddOrUpdateAsync(index);
    }
}