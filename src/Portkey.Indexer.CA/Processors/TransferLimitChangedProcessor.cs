using AElf;
using AElf.Client.Extensions;
using AElf.Types;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class TransferLimitChangedProcessor : CAHolderTransactionEventBase<TransferLimitChanged>
{
    private readonly IAElfIndexerClientEntityRepository<TransferLimitIndex, TransactionInfo> _repository;

    private readonly IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>
        _caHolderTransactionIndexRepository;

    private readonly ContractInfoOptions _contractInfoOptions;
    private readonly IObjectMapper _objectMapper;
    private readonly ILogger<TransferLimitChangedProcessor> _processorLogger;

    public TransferLimitChangedProcessor(ILogger<TransferLimitChangedProcessor> logger,
        IObjectMapper objectMapper, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<TransferLimitIndex, TransactionInfo> repository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>
            caHolderTransactionIndexRepository) : base(logger)
    {
        _objectMapper = objectMapper;
        _repository = repository;
        _processorLogger = logger;
        _contractInfoOptions = contractInfoOptions.Value;
        _caHolderTransactionIndexRepository = caHolderTransactionIndexRepository;
    }

    public override string GetContractAddress(string chainId)
    {
        return _contractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).CAContractAddress;
    }

    protected override async Task HandleEventAsync(TransferLimitChanged eventValue, LogEventContext context)
    {
        var indexId = IdGenerateHelper.GetId(context.ChainId, eventValue.CaHash.ToHex(), nameof(TransferLimitChanged),
            eventValue.Symbol);
        var index = new TransferLimitIndex
        {
            Id = indexId,
            CaHash = eventValue.CaHash.ToHex(),
            Symbol = eventValue.Symbol,
            SingleLimit = eventValue.SingleLimit,
            DailyLimit = eventValue.DailyLimit
        };
        _objectMapper.Map(context, index);
        await _repository.AddOrUpdateAsync(index);

        _processorLogger.LogDebug("[TransferLimitChanged] id: {indexId} CaHash:{CaHash}", index,
            eventValue.CaHash.ToHex());

        var caAddress =
            ConvertVirtualAddressToContractAddress(eventValue.CaHash, GetContractAddress(context.ChainId).ToAddress());
        if (caAddress == null)
        {
            return;
        }

        var transIndex = new CAHolderTransactionIndex
        {
            Id = IdGenerateHelper.GetId(context.BlockHash, context.TransactionId),
            Timestamp = context.BlockTime.ToTimestamp().Seconds,
            FromAddress = caAddress.ToBase58(),
            TransactionFee = GetTransactionFee(context.ExtraProperties),
        };
        _objectMapper.Map(context, transIndex);
        transIndex.MethodName = context.MethodName;
        await _caHolderTransactionIndexRepository.AddOrUpdateAsync(transIndex);
    }
}