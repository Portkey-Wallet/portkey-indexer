using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class LoginGuardianAddedProcessor: LoginGuardianProcessorBase<LoginGuardianAdded>
{
    public LoginGuardianAddedProcessor(ILogger<LoginGuardianAddedProcessor> logger,
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

    protected override async Task HandleEventAsync(LoginGuardianAdded eventValue, LogEventContext context)
    {
        var indexId = IdGenerateHelper.GetId(context.ChainId, eventValue.CaAddress.ToBase58(),
            eventValue.LoginGuardian.IdentifierHash.ToHex(), eventValue.LoginGuardian.VerifierId.ToHex());
        var loginGuardianIndex = await Repository.GetFromBlockStateSetAsync(indexId, context.ChainId);
        if (loginGuardianIndex != null)
        {
            return;
        }
        loginGuardianIndex = new LoginGuardianIndex
        {
            Id = indexId,
            CAHash = eventValue.CaHash.ToHex(),
            CAAddress = eventValue.CaAddress.ToBase58(),
            Manager = eventValue.Manager.ToBase58(),
            LoginGuardian = new Entities.Guardian
            {
                // Guardian = new Entities.Guardian
                // {
                //     Type = (int)eventValue.LoginGuardian.Type,
                //     Verifier = eventValue.LoginGuardian.VerifierId.ToHex()
                // },
                Type = (int)eventValue.LoginGuardian.Type,
                VerifierId = eventValue.LoginGuardian.VerifierId.ToHex(),
                Salt = eventValue.LoginGuardian.Salt,
                IsLoginGuardian = eventValue.LoginGuardian.IsLoginGuardian,
                IdentifierHash = eventValue.LoginGuardian.IdentifierHash.ToHex()
            }
        };
        ObjectMapper.Map(context, loginGuardianIndex);
        await Repository.AddOrUpdateAsync(loginGuardianIndex);
        await AddChangeRecordAsync(loginGuardianIndex.CAAddress, loginGuardianIndex.CAHash,
            loginGuardianIndex.Manager, loginGuardianIndex.LoginGuardian, nameof(LoginGuardianAdded), context);
    }
}