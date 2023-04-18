﻿using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.DependencyInjection;
using Portkey.Indexer.CA.GraphQL;
using Portkey.Indexer.CA.Handlers;
using Portkey.Indexer.CA.Processors;
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
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, ManagerSocialRecoveredProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, NFTMintedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, NFTProtocolCreatedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, NFTTransferredProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, TokenCrossChainTransferredProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, TokenTransferredProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, TokenCrossChainReceivedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, CAHolderCreatedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, LoginGuardianAccountAddedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, LoginGuardianAccountRemovedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, LoginGuardianAccountUnboundProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, ManagerAddedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, ManagerRemovedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, ManagerSocialRecoveredLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, NFTTransferredLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, NFTBurnedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenBurnedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenCrossChainReceivedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenIssuedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenTransferredLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TransactionFeeChargedLogEventProcessor>();

        Configure<ContractInfoOptions>(configuration.GetSection("ContractInfo"));
        Configure<InitialInfoOptions>(configuration.GetSection("InitialInfo"));
        Configure<CAHolderTransactionInfoOptions>(configuration.GetSection("CAHolderTransactionInfo"));
    }

    protected override string ClientId => "AElfIndexer_DApp";
    protected override string Version => "";

}