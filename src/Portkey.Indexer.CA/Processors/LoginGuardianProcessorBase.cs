using AElf.CSharp.Core;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public abstract class LoginGuardianProcessorBase<TEvent> : CAHolderTransactionProcessorBase<TEvent> where TEvent : IEvent<TEvent>, new()
{
    protected readonly IAElfIndexerClientEntityRepository<LoginGuardianIndex, TransactionInfo> Repository;
    protected readonly IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> CaHolderRepository;
    private readonly IAElfIndexerClientEntityRepository<LoginGuardianChangeRecordIndex, TransactionInfo> ChangeRecordRepository;
    protected readonly ContractInfoOptions ContractInfoOptions;
    protected readonly IObjectMapper ObjectMapper;

    protected LoginGuardianProcessorBase(ILogger<LoginGuardianProcessorBase<TEvent>> logger,
        IObjectMapper objectMapper, IAElfIndexerClientEntityRepository<LoginGuardianIndex, TransactionInfo> repository,
        IAElfIndexerClientEntityRepository<LoginGuardianChangeRecordIndex, TransactionInfo> changeRecordRepository,
        IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> caHolderRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo> caHolderTransactionIndexRepository = null,
        IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo> caHolderTransactionAddressIndexRepository = null,
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions = null) : base(logger, 
        caHolderRepository, null, caHolderTransactionIndexRepository,
        null, null, caHolderTransactionAddressIndexRepository,
        contractInfoOptions, caHolderTransactionInfoOptions, objectMapper)
    {
        ObjectMapper = objectMapper;
        Repository = repository;
        ChangeRecordRepository = changeRecordRepository;
        CaHolderRepository = caHolderRepository;
        ContractInfoOptions = contractInfoOptions.Value;
    }

    protected async Task AddChangeRecordAsync(string caAddress, string caHash, string manager, Guardian loginGuardian, string changeType,LogEventContext context)
    {
        var changeRecordId = IdGenerateHelper.GetId(context.ChainId, caAddress,
            loginGuardian.IdentifierHash,changeType,context.TransactionId);
        var changeRecordIndex = await ChangeRecordRepository.GetFromBlockStateSetAsync(changeRecordId, context.ChainId);
        if (changeRecordIndex != null) return;
        changeRecordIndex = new LoginGuardianChangeRecordIndex
        {
            Id = changeRecordId,
            CAAddress = caAddress,
            CAHash = caHash,
            Manager = manager,
            LoginGuardian = loginGuardian,
            ChangeType = changeType
        };
        ObjectMapper.Map(context, changeRecordIndex);
        await ChangeRecordRepository.AddOrUpdateAsync(changeRecordIndex);
    }
}