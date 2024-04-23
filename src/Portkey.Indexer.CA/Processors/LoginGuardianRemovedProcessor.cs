using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class LoginGuardianRemovedProcessor : LoginGuardianProcessorBase<LoginGuardianRemoved>
{
    public LoginGuardianRemovedProcessor(ILogger<LoginGuardianRemovedProcessor> logger,
        IObjectMapper objectMapper, IAElfIndexerClientEntityRepository<LoginGuardianIndex, TransactionInfo> repository,
        IAElfIndexerClientEntityRepository<LoginGuardianChangeRecordIndex, TransactionInfo> changeRecordRepository,
        IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> caHolderRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo> caHolderTransactionIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo> caHolderTransactionAddressIndexRepository,
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions ) : base(logger, objectMapper, repository,
        changeRecordRepository, caHolderRepository, contractInfoOptions, caHolderTransactionIndexRepository,
        caHolderTransactionAddressIndexRepository, caHolderTransactionInfoOptions)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).CAContractAddress;
    }

    protected override async Task HandleEventAsync(LoginGuardianRemoved eventValue, LogEventContext context)
    {
        await HandlerTransactionIndexAsync(eventValue, context);
        
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

        //check ca address if already exist in caHolderIndex
        var id = IdGenerateHelper.GetId(context.ChainId, eventValue.CaAddress.ToBase58());
        var caHolderIndex = await CaHolderRepository.GetFromBlockStateSetAsync(id, context.ChainId);
        if (caHolderIndex == null) return;

        // _objectMapper.Map<LogEventContext, CAHolderIndex>(context, caHolderIndex);

        var guardian = caHolderIndex.Guardians.FirstOrDefault(g =>
            g.IdentifierHash == eventValue.LoginGuardian.IdentifierHash.ToHex() &&
            g.VerifierId == eventValue.LoginGuardian.VerifierId.ToHex() &&
            g.Type == (int)eventValue.LoginGuardian.Type);

        if (guardian == null || !guardian.IsLoginGuardian) return;

        guardian.IsLoginGuardian = false;
        guardian.TransactionId = context.TransactionId;

        ObjectMapper.Map(context, caHolderIndex);
        await CaHolderRepository.AddOrUpdateAsync(caHolderIndex);
    }
    
    protected override async Task HandlerTransactionIndexAsync(LoginGuardianRemoved eventValue, LogEventContext context)
    {
        await ProcessCAHolderTransactionAsync(context, eventValue.CaAddress.ToBase58());;
    }
}