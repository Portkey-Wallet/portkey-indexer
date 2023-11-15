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

public class GuardianUpdatedLogEventProcessor : GuardianProcessorBase<GuardianUpdated>
{
    public GuardianUpdatedLogEventProcessor(ILogger<GuardianUpdatedLogEventProcessor> logger,
        IObjectMapper objectMapper, IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> repository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base(logger, objectMapper, repository,
        contractInfoOptions)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).CAContractAddress;
    }

    protected override async Task HandleEventAsync(GuardianUpdated eventValue, LogEventContext context)
    {
        //check ca address if already exist in caHolderIndex
        var id = IdGenerateHelper.GetId(context.ChainId, eventValue.CaAddress.ToBase58());
        var caHolderIndex = await Repository.GetFromBlockStateSetAsync(id, context.ChainId);

        // _objectMapper.Map<LogEventContext, CAHolderIndex>(context, caHolderIndex);

        var guardian = caHolderIndex?.Guardians.FirstOrDefault(g =>
            g.IdentifierHash == eventValue.GuardianUpdatedPre.IdentifierHash.ToHex() &&
            g.VerifierId == eventValue.GuardianUpdatedPre.VerifierId.ToHex() &&
            g.Type == (int)eventValue.GuardianUpdatedPre.Type);

        if (guardian == null || guardian.VerifierId == eventValue.GuardianUpdatedNew.VerifierId.ToHex()) return;

        guardian.VerifierId = eventValue.GuardianUpdatedNew.VerifierId.ToHex();
        guardian.TransactionId = context.TransactionId;

        ObjectMapper.Map(context, caHolderIndex);
        await Repository.AddOrUpdateAsync(caHolderIndex);
    }
}