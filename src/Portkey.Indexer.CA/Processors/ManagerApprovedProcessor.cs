using AElf.Client.Extensions;
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

public class ManagerApprovedProcessor : CAHolderTransactionEventBase<ManagerApproved>
{
    private readonly IAElfIndexerClientEntityRepository<ManagerApprovedIndex, TransactionInfo> _repository;

    private readonly IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>
        _caHolderTransactionIndexRepository;

    private readonly ContractInfoOptions _contractInfoOptions;
    private readonly IObjectMapper _objectMapper;
    private readonly ILogger<ManagerApprovedProcessor> _logger;

    public ManagerApprovedProcessor(ILogger<ManagerApprovedProcessor> logger,
        IObjectMapper objectMapper, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<ManagerApprovedIndex, TransactionInfo> repository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>
            caHolderTransactionIndexRepository) : base(logger)
    {
        _logger = logger;
        _objectMapper = objectMapper;
        _repository = repository;
        _contractInfoOptions = contractInfoOptions.Value;
        _caHolderTransactionIndexRepository = caHolderTransactionIndexRepository;
    }

    public override string GetContractAddress(string chainId)
    {
        return _contractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).CAContractAddress;
    }

    protected override async Task HandleEventAsync(ManagerApproved eventValue, LogEventContext context)
    {
        var indexId = IdGenerateHelper.GetId(context.ChainId, context.TransactionId);
        var index = new ManagerApprovedIndex
        {
            Id = indexId,
            CaHash = eventValue.CaHash.ToHex(),
            Spender = eventValue.Spender.ToBase58(),
            Symbol = eventValue.Symbol,
            Amount = eventValue.Amount,
        };
        _objectMapper.Map(context, index);
        await _repository.AddOrUpdateAsync(index);

        var caAddress =
            ConvertVirtualAddressToContractAddress(eventValue.CaHash, GetContractAddress(context.ChainId).ToAddress());
        if (caAddress == null)
        {
            return;
        }

        var id = IdGenerateHelper.GetId(context.BlockHash, context.TransactionId);
        var transactionIndex = await _caHolderTransactionIndexRepository.GetFromBlockStateSetAsync(id, context.ChainId);
        var transactionFee = GetTransactionFee(context.ExtraProperties);
        if (transactionIndex != null)
        {
            transactionFee = transactionIndex.TransactionFee.IsNullOrEmpty() ? transactionFee : transactionIndex.TransactionFee;
        }
        
        var transIndex = new CAHolderTransactionIndex
        {
            Id = IdGenerateHelper.GetId(context.BlockHash, context.TransactionId),
            Timestamp = context.BlockTime.ToTimestamp().Seconds,
            FromAddress = caAddress.ToBase58(),
            TransactionFee = transactionFee
        };
        _objectMapper.Map(context, transIndex);
        transIndex.MethodName = context.MethodName;
        await _caHolderTransactionIndexRepository.AddOrUpdateAsync(transIndex);
    }
}