using AElf.Types;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class VirtualTransactionCreatedProcessor : CAHolderTransactionProcessorBase<VirtualTransactionCreated>
{
    public VirtualTransactionCreatedProcessor(
        ILogger<CAHolderTransactionProcessorBase<VirtualTransactionCreated>> logger,
        IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderManagerIndex, TransactionInfo> caHolderManagerIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>
            caHolderTransactionIndexRepository,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, TransactionInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, TransactionInfo> nftInfoIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo>
            caHolderTransactionAddressIndexRepository, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions,
        IObjectMapper objectMapper) : base(logger, caHolderIndexRepository, caHolderManagerIndexRepository,
        caHolderTransactionIndexRepository, tokenInfoIndexRepository, nftInfoIndexRepository,
        caHolderTransactionAddressIndexRepository, contractInfoOptions, caHolderTransactionInfoOptions, objectMapper)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).CAContractAddress;
    }

    private readonly List<string> _skipMethodNames = new(){"transfer", "transferfrom", "approve"};

    protected override async Task HandleEventAsync(VirtualTransactionCreated eventValue, LogEventContext context)
    {
        if (_skipMethodNames.Contains(eventValue.MethodName.ToLower()))
        {
            return;
        }

        var holder = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.From.ToBase58()), context.ChainId);
        if (holder == null) return;
        var id = IdGenerateHelper.GetId(context.BlockHash, context.TransactionId);
        var transIndex = await CAHolderTransactionIndexRepository.GetFromBlockStateSetAsync(id, context.ChainId);
        if (transIndex != null)
        {
            return;
        }

        transIndex = new CAHolderTransactionIndex
        {
            Id = id,
            Timestamp = context.BlockTime.ToTimestamp().Seconds,
            FromAddress = eventValue.From.ToBase58(),
            ToContractAddress = GetToContractAddress(context.ChainId, context.To, context.MethodName, context.Params),
            TransactionFee = GetTransactionFee(context.ExtraProperties),
        };
        ObjectMapper.Map(context, transIndex);
        transIndex.MethodName = eventValue.MethodName;
        await CAHolderTransactionIndexRepository.AddOrUpdateAsync(transIndex);
    }
}