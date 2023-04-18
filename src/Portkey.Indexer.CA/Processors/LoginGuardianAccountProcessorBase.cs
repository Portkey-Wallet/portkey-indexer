using AElf.CSharp.Core;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public abstract class LoginGuardianAccountProcessorBase<TEvent> : AElfLogEventProcessorBase<TEvent,LogEventInfo> where TEvent : IEvent<TEvent>, new()
{
    protected readonly IAElfIndexerClientEntityRepository<LoginGuardianAccountIndex, LogEventInfo> Repository;
    private readonly IAElfIndexerClientEntityRepository<LoginGuardianAccountChangeRecordIndex, LogEventInfo> ChangeRecordRepository;
    protected readonly ContractInfoOptions ContractInfoOptions;
    protected readonly IObjectMapper ObjectMapper;

    protected LoginGuardianAccountProcessorBase(ILogger<LoginGuardianAccountProcessorBase<TEvent>> logger,
        IObjectMapper objectMapper, IAElfIndexerClientEntityRepository<LoginGuardianAccountIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<LoginGuardianAccountChangeRecordIndex, LogEventInfo> changeRecordRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base(logger)
    {
        ObjectMapper = objectMapper;
        Repository = repository;
        ChangeRecordRepository = changeRecordRepository;
        ContractInfoOptions = contractInfoOptions.Value;
    }

    protected async Task AddChangeRecordAsync(string caAddress, string caHash, string manager, GuardianAccount loginGuardianAccount, string changeType,LogEventContext context)
    {
        var changeRecordId = IdGenerateHelper.GetId(context.ChainId, caAddress,
            loginGuardianAccount.Value,changeType,context.TransactionId);
        var changeRecordIndex = await ChangeRecordRepository.GetFromBlockStateSetAsync(changeRecordId, context.ChainId);
        if (changeRecordIndex != null) return;
        changeRecordIndex = new LoginGuardianAccountChangeRecordIndex
        {
            Id = changeRecordId,
            CAAddress = caAddress,
            CAHash = caHash,
            Manager = manager,
            LoginGuardianAccount = loginGuardianAccount,
            ChangeType = changeType
        };
        ObjectMapper.Map(context, changeRecordIndex);
        await ChangeRecordRepository.AddOrUpdateAsync(changeRecordIndex);
    }
}