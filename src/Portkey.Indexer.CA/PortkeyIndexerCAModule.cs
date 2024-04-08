using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.DependencyInjection;
using Portkey.Indexer.CA.GraphQL;
using Portkey.Indexer.CA.Handlers;
using Portkey.Indexer.CA.Processors;
using Portkey.Indexer.CA.Provider;
using Volo.Abp.Modularity;

namespace Portkey.Indexer.CA;


[DependsOn(typeof(AElfIndexerClientModule))]
public class PortkeyIndexerCAModule:AElfIndexerClientPluginBaseModule<PortkeyIndexerCAModule, PortKeyIndexerCASchema, Query>
{
    protected override void ConfigureServices(IServiceCollection serviceCollection)
    {
        var configuration = serviceCollection.GetConfiguration();
        serviceCollection.AddTransient<IBlockChainDataHandler, CAHolderTransactionHandler>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, TokenCreatedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, ContractDeployedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, ManagerSocialRecoveredProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, TokenApprovedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, CAHolderCreatedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, GuardianAddedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, GuardianRemovedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, GuardianUpdatedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, LoginGuardianAddedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, LoginGuardianRemovedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, LoginGuardianUnboundLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, ManagerAddedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, ManagerRemovedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, ManagerUpdatedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, TokenBurnedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, TokenCrossChainReceivedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, TokenIssuedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, TokenTransferredLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, TransactionFeeChargedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, CAHolderSyncedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, InviteLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, BingoedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, RegisteredProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, PlayedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, VirtualTransactionCreatedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, TransferSecurityThresholdChangedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, TransferLimitChangedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, ManagerApprovedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, CAHolderAccelerateCreationProcessor>();
        serviceCollection.AddSingleton<IAElfDataProvider, AElfDataProvider>();

        Configure<ContractInfoOptions>(configuration.GetSection("ContractInfo"));
        Configure<InitialInfoOptions>(configuration.GetSection("InitialInfo"));
        Configure<CAHolderTransactionInfoOptions>(configuration.GetSection("CAHolderTransactionInfo"));
        Configure<InscriptionListOptions>(configuration.GetSection("InscriptionList"));
    }

    protected override string ClientId => "";
    protected override string Version => "";

    
}