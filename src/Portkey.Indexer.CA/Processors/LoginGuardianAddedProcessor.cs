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

public class LoginGuardianAddedProcessor : LoginGuardianProcessorBase<LoginGuardianAdded>
{
    public LoginGuardianAddedProcessor(ILogger<LoginGuardianAddedProcessor> logger,
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

    protected override async Task HandleEventAsync(LoginGuardianAdded eventValue, LogEventContext context)
    {
        await HandlerTransactionIndexAsync(eventValue, context);
        
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

        //check ca address if already exist in caHolderIndex
        var id = IdGenerateHelper.GetId(context.ChainId, eventValue.CaAddress.ToBase58());
        var caHolderIndex = await CaHolderRepository.GetFromBlockStateSetAsync(id, context.ChainId);
        if (caHolderIndex == null) return;

        // _objectMapper.Map<LogEventContext, CAHolderIndex>(context, caHolderIndex);

        var guardian = caHolderIndex.Guardians.FirstOrDefault(g =>
            g.IdentifierHash == eventValue.LoginGuardian.IdentifierHash.ToHex() &&
            g.VerifierId == eventValue.LoginGuardian.VerifierId.ToHex() &&
            g.Type == (int)eventValue.LoginGuardian.Type);

        if (guardian == null)
        {
            guardian = ObjectMapper.Map<Guardian, Entities.Guardian>(eventValue.LoginGuardian);
            guardian.TransactionId = context.TransactionId;
            caHolderIndex.Guardians.Add(guardian);
        }
        else
        {
            if (guardian.IsLoginGuardian) return;
            guardian.IsLoginGuardian = true;
        }

        ObjectMapper.Map(context, caHolderIndex);
        await CaHolderRepository.AddOrUpdateAsync(caHolderIndex);
    }
    
    protected override async Task HandlerTransactionIndexAsync(LoginGuardianAdded eventValue, LogEventContext context)
    {
        await ProcessCAHolderTransactionAsync(context, eventValue.CaAddress.ToBase58());;
    }
}