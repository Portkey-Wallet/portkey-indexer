using AElf.CSharp.Core;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public abstract class GuardianProcessorBase<TEvent> : CAHolderTransactionProcessorBase<TEvent>
    where TEvent : IEvent<TEvent>, new()
{
    protected readonly IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> Repository;
    protected readonly ContractInfoOptions ContractInfoOptions;
    protected readonly IObjectMapper ObjectMapper;
    private IAElfIndexerClientEntityRepository<GuardianChangeRecordIndex, TransactionInfo> ChangeRecordRepository;

    protected GuardianProcessorBase(ILogger<GuardianProcessorBase<TEvent>> logger,
        IObjectMapper objectMapper, IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> repository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<GuardianChangeRecordIndex, TransactionInfo> changeRecordRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo> caHolderTransactionIndexRepository = null,
        IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo> caHolderTransactionAddressIndexRepository = null,
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions = null) : base(logger, 
        repository, null, caHolderTransactionIndexRepository,
        null, null, caHolderTransactionAddressIndexRepository,
        contractInfoOptions, caHolderTransactionInfoOptions, objectMapper)
    {
        ObjectMapper = objectMapper;
        Repository = repository;
        ContractInfoOptions = contractInfoOptions.Value;
        ChangeRecordRepository = changeRecordRepository;
    }

    protected async Task AddChangeRecordAsync(string caAddress, string caHash,
        string changeType, Guardian guardian, LogEventContext context)
    {
        var changeRecordId = IdGenerateHelper.GetId(context.ChainId, caAddress, context.TransactionId);
        var changeRecordIndex = await ChangeRecordRepository.GetFromBlockStateSetAsync(changeRecordId, context.ChainId);
        if (changeRecordIndex != null) return;
        changeRecordIndex = new GuardianChangeRecordIndex
        {
            Id = changeRecordId,
            ChangeType = changeType,
            CAAddress = caAddress,
            CAHash = caHash,
            Guardian = guardian
        };
        ObjectMapper.Map(context, changeRecordIndex);
        await ChangeRecordRepository.AddOrUpdateAsync(changeRecordIndex);
    }
}