using AElf.CSharp.Core;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public abstract class CAHolderManagerProcessorBase<TEvent> : CAHolderTransactionProcessorBase<TEvent> where TEvent : IEvent<TEvent>, new()
{
    protected readonly IObjectMapper ObjectMapper;
    protected readonly IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> Repository;
    protected readonly IAElfIndexerClientEntityRepository<CAHolderManagerIndex, TransactionInfo> CAHolderManagerIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<CAHolderManagerChangeRecordIndex, TransactionInfo> ChangeRecordRepository;
    protected readonly ContractInfoOptions ContractInfoOptions;
    
    protected CAHolderManagerProcessorBase(ILogger<CAHolderManagerProcessorBase<TEvent>> logger, IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> repository,
        IAElfIndexerClientEntityRepository<CAHolderManagerIndex, TransactionInfo> caHolderManagerIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderManagerChangeRecordIndex, TransactionInfo> changeRecordRepository,
        // transactionIndex needs
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo> caHolderTransactionIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo> caHolderTransactionAddressIndexRepository,
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions
        ) :
        base(logger, 
            repository, caHolderManagerIndexRepository, caHolderTransactionIndexRepository,
            null, null, caHolderTransactionAddressIndexRepository,
            contractInfoOptions, caHolderTransactionInfoOptions, objectMapper)
    {
        ObjectMapper = objectMapper;
        Repository = repository;
        CAHolderManagerIndexRepository = caHolderManagerIndexRepository;
        ChangeRecordRepository = changeRecordRepository;
        ContractInfoOptions = contractInfoOptions.Value;
    }

    protected async Task AddChangeRecordAsync(string caAddress, string caHash, string manager, string changeType,
        LogEventContext context)
    {
        var changeRecordId = IdGenerateHelper.GetId(context.ChainId, caAddress,
            manager, context.TransactionId);
        var changeRecordIndex = await ChangeRecordRepository.GetFromBlockStateSetAsync(changeRecordId, context.ChainId);
        if (changeRecordIndex != null) return;
        changeRecordIndex = new CAHolderManagerChangeRecordIndex
        {
            Id = changeRecordId,
            Manager = manager,
            ChangeType = changeType,
            CAAddress = caAddress,
            CAHash = caHash
        };
        ObjectMapper.Map(context, changeRecordIndex);
        await ChangeRecordRepository.AddOrUpdateAsync(changeRecordIndex);
    }
}