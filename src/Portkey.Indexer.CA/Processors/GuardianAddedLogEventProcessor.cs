using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;
using Guardian = Portkey.Contracts.CA.Guardian;

namespace Portkey.Indexer.CA.Processors;

public class GuardianAddedLogEventProcessor : GuardianProcessorBase<GuardianAdded>
{
    public GuardianAddedLogEventProcessor(ILogger<GuardianAddedLogEventProcessor> logger,
        IObjectMapper objectMapper, IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> repository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<GuardianChangeRecordIndex, LogEventInfo> changeRecordRepository) : base(
        logger, objectMapper, repository,
        contractInfoOptions, changeRecordRepository)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).CAContractAddress;
    }

    protected override async Task HandleEventAsync(GuardianAdded eventValue, LogEventContext context)
    {
        //check ca address if already exist in caHolderIndex
        var id = IdGenerateHelper.GetId(context.ChainId, eventValue.CaAddress.ToBase58());
        var caHolderIndex = await Repository.GetFromBlockStateSetAsync(id, context.ChainId);
        if (caHolderIndex == null) return;

        // _objectMapper.Map<LogEventContext, CAHolderIndex>(context, caHolderIndex);

        var guardian = caHolderIndex.Guardians.FirstOrDefault(g =>
            g.IdentifierHash == eventValue.GuardianAdded_.IdentifierHash.ToHex() &&
            g.VerifierId == eventValue.GuardianAdded_.VerifierId.ToHex() &&
            g.Type == (int)eventValue.GuardianAdded_.Type);

        if (guardian != null) return;

        var guardianAdded = ObjectMapper.Map<Guardian, Entities.Guardian>(eventValue.GuardianAdded_);
        caHolderIndex.Guardians.Add(guardianAdded);

        ObjectMapper.Map(context, caHolderIndex);
        await Repository.AddOrUpdateAsync(caHolderIndex);
        
        await AddChangeRecordAsync(eventValue.CaAddress.ToBase58(), eventValue.CaHash.ToHex(), nameof(GuardianAdded),
            nameof(GuardianAdded), guardianAdded, context);
    }
}