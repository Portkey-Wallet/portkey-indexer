using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class LoginGuardianUnboundLogEventProcessor : LoginGuardianProcessorBase<LoginGuardianUnbound>
{
    public LoginGuardianUnboundLogEventProcessor(ILogger<LoginGuardianUnboundLogEventProcessor> logger,
        IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<LoginGuardianIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<LoginGuardianChangeRecordIndex, LogEventInfo> changeRecordRepository,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> caHolderRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base(logger, objectMapper, repository,
        changeRecordRepository, caHolderRepository, contractInfoOptions)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).CAContractAddress;
    }

    protected override async Task HandleEventAsync(LoginGuardianUnbound eventValue, LogEventContext context)
    {
        await AddChangeRecordAsync(eventValue.CaAddress.ToBase58(), eventValue.CaHash.ToHex(),
            eventValue.Manager.ToBase58(), new Entities.Guardian
            {
                IdentifierHash = eventValue.LoginGuardianIdentifierHash.ToHex()
            }, nameof(LoginGuardianUnbound), context,false);
    }
}