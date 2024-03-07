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
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenCreatedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, ContractDeployedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, ManagerAddedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, ManagerRemovedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, ManagerUpdatedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, ManagerSocialRecoveredProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, CAHolderCreatedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, GuardianAddedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, GuardianRemovedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, GuardianUpdatedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, LoginGuardianAddedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, LoginGuardianRemovedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, LoginGuardianUnboundProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, TokenApprovedProcessor>();
        // serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, NFTMintedProcessor>();
        // serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, NFTProtocolCreatedProcessor>();
        // serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, NFTTransferredProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, TokenCrossChainTransferredProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, TokenTransferredProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, TokenCrossChainReceivedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, CAHolderCreatedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, GuardianAddedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, GuardianRemovedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, GuardianUpdatedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, LoginGuardianAddedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, LoginGuardianRemovedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, LoginGuardianUnboundLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, ManagerAddedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, ManagerRemovedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, ManagerUpdatedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, ManagerSocialRecoveredLogEventProcessor>();
        // serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, NFTTransferredLogEventProcessor>();
        // serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, NFTBurnedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenBurnedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenCrossChainReceivedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenIssuedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenTransferredLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TransactionFeeChargedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, CAHolderSyncedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, InviteLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, BingoedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, RegisteredProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, PlayedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, VirtualTransactionCreatedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TransferSecurityThresholdChangedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, TransferLimitChangedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, ManagerApprovedProcessor>();
        serviceCollection
            .AddSingleton<IAElfLogEventProcessor<LogEventInfo>, CAHolderAccelerateCreationLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, CAHolderAccelerateCreationProcessor>();
        serviceCollection.AddSingleton<IAElfDataProvider, AElfDataProvider>();

        Configure<ContractInfoOptions>(configuration.GetSection("ContractInfo"));
        Configure<InitialInfoOptions>(configuration.GetSection("InitialInfo"));
        Configure<CAHolderTransactionInfoOptions>(configuration.GetSection("CAHolderTransactionInfo"));
        Configure<CAHolderTransactionInfoOptions>(configuration.GetSection("InscriptionList"));
    }

    protected override string ClientId => "Portkey_DID";
    protected override string Version => "";

    
}