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

    protected override async Task HandleEventAsync(LoginGuardianUnbound eventValue, LogEventContext context)
    {
        await HandlerTransactionIndexAsync(eventValue, context);
        
        await AddChangeRecordAsync(eventValue.CaAddress.ToBase58(), eventValue.CaHash.ToHex(),
            eventValue.Manager.ToBase58(), new Entities.Guardian
            {
                IdentifierHash = eventValue.LoginGuardianIdentifierHash.ToHex()
            }, nameof(LoginGuardianUnbound), context);
    }
    
    protected override async Task HandlerTransactionIndexAsync(LoginGuardianUnbound eventValue, LogEventContext context)
    {
        await ProcessCAHolderTransactionAsync(context, eventValue.CaAddress.ToBase58());;
    }
}