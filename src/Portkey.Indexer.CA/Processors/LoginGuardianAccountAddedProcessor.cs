using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class LoginGuardianAccountAddedProcessor: LoginGuardianAccountProcessorBase<LoginGuardianAccountAdded>
{
    public LoginGuardianAccountAddedProcessor(ILogger<LoginGuardianAccountAddedProcessor> logger,
        IObjectMapper objectMapper, IAElfIndexerClientEntityRepository<LoginGuardianAccountIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<LoginGuardianAccountChangeRecordIndex, LogEventInfo> changeRecordRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base(logger, objectMapper, repository,
        changeRecordRepository, contractInfoOptions)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).CAContractAddress;
    }

    protected override async Task HandleEventAsync(LoginGuardianAccountAdded eventValue, LogEventContext context)
    {
        var indexId = IdGenerateHelper.GetId(context.ChainId, eventValue.CaAddress.ToBase58(),
            eventValue.LoginGuardianAccount.Value, eventValue.LoginGuardianAccount.Guardian.Verifier.Id.ToHex());
        var loginGuardianAccountIndex = await Repository.GetFromBlockStateSetAsync(indexId, context.ChainId);
        if (loginGuardianAccountIndex != null)
        {
            return;
        }
        loginGuardianAccountIndex = new LoginGuardianAccountIndex
        {
            Id = indexId,
            CAHash = eventValue.CaHash.ToHex(),
            CAAddress = eventValue.CaAddress.ToBase58(),
            Manager = eventValue.Manager.ToBase58(),
            LoginGuardianAccount = new Entities.GuardianAccount
            {
                Guardian = new Entities.Guardian
                {
                    Type = (int)eventValue.LoginGuardianAccount.Guardian.Type,
                    Verifier = eventValue.LoginGuardianAccount.Guardian.Verifier.Id.ToHex()
                },
                Value = eventValue.LoginGuardianAccount.Value
            }
        };
        ObjectMapper.Map(context, loginGuardianAccountIndex);
        await Repository.AddOrUpdateAsync(loginGuardianAccountIndex);
        await AddChangeRecordAsync(loginGuardianAccountIndex.CAAddress, loginGuardianAccountIndex.CAHash,
            loginGuardianAccountIndex.Manager, loginGuardianAccountIndex.LoginGuardianAccount, nameof(LoginGuardianAccountAdded), context);
    }
}