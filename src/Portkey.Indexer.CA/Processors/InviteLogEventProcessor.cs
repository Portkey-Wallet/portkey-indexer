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

public class InviteLogEventProcessor : AElfLogEventProcessorBase<Invited, TransactionInfo>
{
    private readonly IObjectMapper _objectMapper;
    private readonly IAElfIndexerClientEntityRepository<InviteIndex, TransactionInfo> _repository;
    private readonly ContractInfoOptions _contractInfoOptions;

    public InviteLogEventProcessor(ILogger<InviteLogEventProcessor> logger, IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<InviteIndex, TransactionInfo> repository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base(logger)
    {
        _objectMapper = objectMapper;
        _repository = repository;
        _contractInfoOptions = contractInfoOptions.Value;
    }

    public override string GetContractAddress(string chainId)
    {
        return _contractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).CAContractAddress;
    }

    protected override async Task HandleEventAsync(Invited eventValue, LogEventContext context)
    {
        if (eventValue.ProjectCode.IsNullOrEmpty())
        {
            return;
        }

        var indexId = string.Empty;
        if (eventValue.MethodName == CommonConstant.CreateCAHolder)
        {
            indexId = IdGenerateHelper.GetId(eventValue.MethodName, eventValue.ProjectCode, eventValue.CaHash.ToHex());
        }
        else
        {
            indexId = IdGenerateHelper.GetId(eventValue.MethodName, eventValue.ProjectCode, eventValue.ReferralCode,
                eventValue.CaHash.ToHex());
        }

        var inviteIndex = await _repository.GetFromBlockStateSetAsync(indexId, context.ChainId);
        if (inviteIndex != null)
        {
            return;
        }

        inviteIndex = new InviteIndex
        {
            Id = indexId,
            CaHash = eventValue.CaHash.ToHex(),
            ProjectCode = eventValue.ProjectCode,
            ReferralCode = eventValue.ReferralCode,
            Timestamp = context.BlockTime.ToTimestamp().Seconds,
        };
        _objectMapper.Map(context, inviteIndex);

        inviteIndex.MethodName = eventValue.MethodName;
        await _repository.AddOrUpdateAsync(inviteIndex);
    }
}