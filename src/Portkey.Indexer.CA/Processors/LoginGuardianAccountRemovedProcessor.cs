using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class LoginGuardianAccountRemovedProcessor: LoginGuardianAccountProcessorBase<LoginGuardianAccountRemoved>
{
    public LoginGuardianAccountRemovedProcessor(ILogger<LoginGuardianAccountRemovedProcessor> logger,
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
    
    protected override async Task HandleEventAsync(LoginGuardianAccountRemoved eventValue, LogEventContext context)
    {
        var indexId = IdGenerateHelper.GetId(context.ChainId, eventValue.CaAddress.ToBase58(),
            eventValue.LoginGuardianAccount.Value, eventValue.LoginGuardianAccount.Guardian.Verifier.Id.ToHex());
        var loginGuardianAccountIndex = await Repository.GetAsync(indexId);
        if (loginGuardianAccountIndex == null)
        {
            return;
        }
        
        ObjectMapper.Map(context, loginGuardianAccountIndex);
        await Repository.DeleteAsync(loginGuardianAccountIndex);
        await AddChangeRecordAsync(loginGuardianAccountIndex.CAAddress, loginGuardianAccountIndex.CAHash,
            loginGuardianAccountIndex.Manager, loginGuardianAccountIndex.LoginGuardianAccount, nameof(LoginGuardianAccountRemoved), context);
    }
}