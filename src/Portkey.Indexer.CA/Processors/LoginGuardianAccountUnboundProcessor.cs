using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class LoginGuardianAccountUnboundProcessor : LoginGuardianAccountProcessorBase<LoginGuardianAccountUnbound>
{
    public LoginGuardianAccountUnboundProcessor(ILogger<LoginGuardianAccountUnboundProcessor> logger,
        IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<LoginGuardianAccountIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<LoginGuardianAccountChangeRecordIndex, LogEventInfo> changeRecordRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base(logger, objectMapper, repository,
        changeRecordRepository, contractInfoOptions)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).CAContractAddress;
    }

    protected override async Task HandleEventAsync(LoginGuardianAccountUnbound eventValue, LogEventContext context)
    {
        await AddChangeRecordAsync(eventValue.CaAddress.ToBase58(), eventValue.CaHash.ToHex(),
            eventValue.Manager.ToBase58(), new Entities.GuardianAccount
            {
                Value = eventValue.LoginGuardianAccount
            }, nameof(LoginGuardianAccountUnbound), context);
    }
}