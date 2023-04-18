using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class LoginGuardianRemovedProcessor: LoginGuardianProcessorBase<LoginGuardianRemoved>
{
    public LoginGuardianRemovedProcessor(ILogger<LoginGuardianRemovedProcessor> logger,
        IObjectMapper objectMapper, IAElfIndexerClientEntityRepository<LoginGuardianIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<LoginGuardianChangeRecordIndex, LogEventInfo> changeRecordRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base(logger, objectMapper, repository,
        changeRecordRepository, contractInfoOptions)
    {
    }
    
    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).CAContractAddress;
    }
    
    protected override async Task HandleEventAsync(LoginGuardianRemoved eventValue, LogEventContext context)
    {
        var indexId = IdGenerateHelper.GetId(context.ChainId, eventValue.CaAddress.ToBase58(),
            eventValue.LoginGuardian.IdentifierHash.ToHex(), eventValue.LoginGuardian.VerifierId.ToHex());
        var loginGuardianIndex = await Repository.GetAsync(indexId);
        if (loginGuardianIndex == null)
        {
            return;
        }
        
        ObjectMapper.Map(context, loginGuardianIndex);
        await Repository.DeleteAsync(loginGuardianIndex);
        await AddChangeRecordAsync(loginGuardianIndex.CAAddress, loginGuardianIndex.CAHash,
            loginGuardianIndex.Manager, loginGuardianIndex.LoginGuardian, nameof(LoginGuardianRemoved), context);
    }
}