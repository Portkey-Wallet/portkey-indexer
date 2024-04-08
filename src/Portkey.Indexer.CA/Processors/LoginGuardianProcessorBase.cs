using AElf.CSharp.Core;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public abstract class LoginGuardianProcessorBase<TEvent> : AElfLogEventProcessorBase<TEvent,LogEventInfo> where TEvent : IEvent<TEvent>, new()
{
    protected readonly IAElfIndexerClientEntityRepository<LoginGuardianIndex, LogEventInfo> Repository;
    protected readonly IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> CaHolderRepository;
    private readonly IAElfIndexerClientEntityRepository<LoginGuardianChangeRecordIndex, LogEventInfo> ChangeRecordRepository;
    protected readonly ContractInfoOptions ContractInfoOptions;
    protected readonly IObjectMapper ObjectMapper;

    protected LoginGuardianProcessorBase(ILogger<LoginGuardianProcessorBase<TEvent>> logger,
        IObjectMapper objectMapper, IAElfIndexerClientEntityRepository<LoginGuardianIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<LoginGuardianChangeRecordIndex, LogEventInfo> changeRecordRepository,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> caHolderRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base(logger)
    {
        ObjectMapper = objectMapper;
        Repository = repository;
        ChangeRecordRepository = changeRecordRepository;
        CaHolderRepository = caHolderRepository;
        ContractInfoOptions = contractInfoOptions.Value;
    }

    protected async Task AddChangeRecordAsync(string caAddress, string caHash, string manager, Guardian loginGuardian, string changeType, LogEventContext context, bool isCreateHolder)
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
            ChangeType = changeType,
            IsCreateHolder = isCreateHolder
        };
        ObjectMapper.Map(context, changeRecordIndex);
        await ChangeRecordRepository.AddOrUpdateAsync(changeRecordIndex);
    }
}