using System.Reflection;
using AElf.Indexing.Elasticsearch;
using AElf.Indexing.Elasticsearch.Options;
using AElf.Indexing.Elasticsearch.Services;
using AElf.Types;
using AElfIndexer.BlockScan;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Client.Providers;
using AElfIndexer.Grains.Grain.Client;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Nethereum.Hex.HexConvertors.Extensions;
using Portkey.Indexer.CA.Provider;
using Portkey.Indexer.CA.Tests.Provider;
using Portkey.Indexer.Orleans.TestBase;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;

namespace Portkey.Indexer.CA.Tests;

[DependsOn(
    // typeof(AElfIndexerClientModule),
    // typeof(AElfIndexerApplicationModule),
    typeof(PortkeyIndexerOrleansTestBaseModule),
    typeof(PortkeyIndexerCAModule),
    typeof(AbpAutoMapperModule),
    typeof(AbpAutofacModule),
    typeof(AElfIndexingElasticsearchModule))]
public class PortkeyIndexerCATestModule : AbpModule
{
    private string ClientId { get; } = "TestPortkeyClient";
    private string Version { get; } = "TestPortkeyVersion";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var mockEventbus = new Mock<IDistributedEventBus>();
        mockEventbus.Setup(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(Task.CompletedTask);
        context.Services.AddSingleton(mockEventbus.Object);
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<PortkeyIndexerCATestModule>(); });
        context.Services.AddSingleton<IAElfIndexerClientInfoProvider, AElfIndexerClientInfoProvider>();
        context.Services.AddSingleton<ISubscribedBlockHandler, SubscribedBlockHandler>();
        context.Services.AddTransient<IBlockChainDataHandler, LogEventDataHandler>();
        context.Services.AddTransient(typeof(IAElfIndexerClientEntityRepository<,>),
            typeof(AElfIndexerClientEntityRepository<,>));
        context.Services.AddSingleton(typeof(IBlockStateSetProvider<>), typeof(BlockStateSetProvider<>));
        context.Services.AddSingleton<IDAppDataProvider, DAppDataProvider>();
        context.Services.AddSingleton(typeof(IDAppDataIndexProvider<>), typeof(DAppDataIndexProvider<>));
        context.Services.AddSingleton<IAElfClientProvider, AElfClientProvider>();
        context.Services.AddSingleton<IAElfDataProvider, MockAElfDataProvider>();

        context.Services.Configure<ClientOptions>(o => { o.DAppDataCacheCount = 5; });

        context.Services.Configure<NodeOptions>(o =>
        {
            o.NodeConfigList = new List<NodeConfig>
            {
                new NodeConfig { ChainId = "AELF", Endpoint = "http://mainchain.io" }
            };
        });

        context.Services.Configure<EsEndpointOption>(options =>
        {
            options.Uris = new List<string> { "http://127.0.0.1:9200" };
        });

        context.Services.Configure<IndexSettingOptions>(options =>
        {
            options.NumberOfReplicas = 1;
            options.NumberOfShards = 1;
            options.Refresh = Refresh.True;
            options.IndexPrefix = "AElfIndexer";
        });

        context.Services.Configure<CAHolderTransactionInfoOptions>(options =>
        {
            options.CAHolderTransactionInfos = new List<CAHolderTransactionInfo>
            {
                new ()
                {
                    ChainId = "AELF",
                    ContractAddress = "CAAddress",
                    MethodName = "AddGuardian",
                    EventNames = new List<string>
                    {
                        "GuardianAdded"
                    }
                },
                new ()
                {
                    ChainId = "AELF",
                    ContractAddress = "CAAddress",
                    MethodName = "RemoveGuardian",
                    EventNames = new List<string>
                    {
                        "GuardianRemoved"
                    }
                },
                new ()
                {
                    ChainId = "AELF",
                    ContractAddress = "CAAddress",
                    MethodName = "UpdateGuardian",
                    EventNames = new List<string>
                    {
                        "GuardianUpdated"
                    }
                },
                new ()
                {
                    ChainId = "AELF",
                    ContractAddress = "CAAddress",
                    MethodName = "SetGuardianForLogin",
                    EventNames = new List<string>
                    {
                        "LoginGuardianAdded"
                    }
                },
                new ()
                {
                    ChainId = "AELF",
                    ContractAddress = "CAAddress",
                    MethodName = "UnsetGuardianForLogin",
                    EventNames = new List<string>
                    {
                        "LoginGuardianRemoved"
                    }
                },
                new ()
                {
                    ChainId = "AELF",
                    ContractAddress = "CAAddress",
                    MethodName = "UnsetGuardianForLogin",
                    EventNames = new List<string>
                    {
                        "LoginGuardianUnbound"
                    }
                },
                new ()
                {
                    ChainId = "AELF",
                    ContractAddress = "TokenAddress",
                    MethodName = "Approve",
                    EventNames = new List<string>
                    {
                        "Approved"
                    }
                },
                new ()
                {
                    ChainId = "AELF",
                    ContractAddress = "CAAddress",
                    MethodName = "AddManagerInfo",
                    EventNames = new List<string>
                    {
                        "ManagerInfoAdded"
                    }
                },
                new ()
                {
                    ChainId = "AELF",
                    ContractAddress = "CAAddress",
                    MethodName = "RemoveManagerInfo",
                    EventNames = new List<string>
                    {
                        "ManagerInfoRemoved"
                    }
                },
                new ()
                {
                    ChainId = "AELF",
                    ContractAddress = "CAAddress",
                    MethodName = "RemoveOtherManagerInfo",
                    EventNames = new List<string>
                    {
                        "ManagerInfoRemoved"
                    }
                },
                new ()
                {
                    ChainId = "AELF",
                    ContractAddress = "CAAddress",
                    MethodName = "UpdateManagerInfo",
                    EventNames = new List<string>
                    {
                        "ManagerInfoUpdated"
                    }
                },
                new ()
                {
                    ChainId = "AELF",
                    ContractAddress = "CAAddress",
                    MethodName = "SocialRecovery",
                    EventNames = new List<string>
                    {
                        "ManagerInfoSocialRecovered"
                    }
                },
                new ()
                {
                    ChainId = "AELF",
                    ContractAddress = Address.FromPublicKey("AAA".HexToByteArray()).ToString(),
                    MethodName = "AddManagerInfo",
                    EventNames = new List<string> { "CrossChainReceived" ,"Transferred"}
                },
                new ()
                {
                    ChainId = "AELF",
                    ContractAddress = "CAAddress",
                    MethodName = "CrossChainTransfer",
                    EventNames = new List<string> { "CrossChainTransferred"}
                },
                new ()
                {
                    ChainId = "tDVV",
                    ContractAddress = "CAAddress",
                    MethodName = "CrossChainReceiveToken",
                    EventNames = new List<string> { "CrossChainReceived" }
                },
                new ()
                {
                    ChainId = "AELF",
                    ContractAddress = "CAAddress",
                    MethodName = "Transferred",
                    EventNames = new List<string> { "Transferred" }
                },
                new ()
                {
                    ChainId = "AELF",
                    ContractAddress = "CAAddress",
                    MethodName = "CreateHolderInfo",
                    EventNames = new List<string> { "CAHolderCreated" }
                },
                new ()
                {
                    ChainId = "AELF",
                    ContractAddress = "CAAddress",
                    MethodName = "Bingoed",
                    EventNames = new List<string> { "Bingoed" }
                },
                new ()
                {
                    ChainId = "AELF",
                    ContractAddress = "CAAddress",
                    MethodName = "Played",
                    EventNames = new List<string> { "Played" }
                },
                new ()
                {
                    ChainId = "AELF",
                    ContractAddress = "CAAddress",
                    MethodName = "Bingo",
                    EventNames = new List<string> { "Bingoed" }
                },
                new ()
                {
                    ChainId = "AELF",
                    ContractAddress = "CAAddress",
                    MethodName = "Play",
                    EventNames = new List<string> { "Played" }
                },
                new ()
                {
                    ChainId = "AELF",
                    ContractAddress = "CAAddress",
                    MethodName = "Registered",
                    EventNames = new List<string> { "Registered" }
                },
                new ()
                {
                    ChainId = "AELF",
                    ContractAddress = "aLyxCJvWMQH6UEykTyeWAcYss9baPyXkrMQ37BHnUicxD2LL3",
                    MethodName = "ManagerForwardCall",
                    EventNames = new List<string>
                    {
                        "Transferred"
                    }
                },
                new ()
                {
                    ChainId = "AELF",
                    ContractAddress = Address.FromPublicKey("ABC".HexToByteArray()).ToBase58(),
                    MethodName = "TransferToken",
                    EventNames = new List<string>
                    {
                        "Transferred"
                    }
                },
            };
        });
        
        context.Services.Configure<ContractInfoOptions>(options =>
        {
            options.ContractInfos = new List<ContractInfo>
            {
                new ()
                {
                    ChainId = "AELF",
                    CAContractAddress = "aLyxCJvWMQH6UEykTyeWAcYss9baPyXkrMQ37BHnUicxD2LL3",
                    GenesisContractAddress = "genesis",
                    NFTContractAddress = "nft",
                    TokenContractAddress = "token"
                },
                new ()
                {
                    ChainId = "AELF",
                    CAContractAddress = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
                },
                new ()
                {
                    ChainId = "tDVV",
                    CAContractAddress = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
                },
                new ()
                {
                    ChainId = "AELF",
                    CAContractAddress = "CAAddress"
                }
            };
        });
        
        context.Services.Configure<InitialInfoOptions>(options =>
        {
            options.TokenInfoList = new List<TokenInfo>
            {
                new TokenInfo
                {
                    ChainId = "AELF", 
                    Symbol = "SYB",
                    Decimals = 8,
                    TokenContractAddress = "token",
                    TotalSupply = 10000000000000000,
                    Issuer = "issuer",
                    IsBurnable = true,
                    TokenName =  "SYB Token",
                    IssueChainId = 992731,
                }
            };
        });
        
        context.Services.Configure<InscriptionListOptions>(options =>
        {
            options.Inscriptions = new List<string>
            {
                "READ-0",
                "WRITE-0"
            };
        });

        var applicationBuilder = new ApplicationBuilder(context.Services.BuildServiceProvider());
        context.Services.AddObjectAccessor<IApplicationBuilder>(applicationBuilder);
        var mockBlockScanAppService = new Mock<IBlockScanAppService>();
        mockBlockScanAppService.Setup(p => p.GetMessageStreamIdsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(new List<Guid>()));
        context.Services.AddSingleton<IBlockScanAppService>(mockBlockScanAppService.Object);
        // context.Services.AddSingleton<IClusterClient>((new Mock<IClusterClient>()).Object);
    }

    public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
    {
        var provider = context.ServiceProvider.GetRequiredService<IAElfIndexerClientInfoProvider>();
        provider.SetClientId(ClientId);
        provider.SetVersion(Version);
        AsyncHelper.RunSync(async () =>
            await CreateIndexAsync(context.ServiceProvider)
        );
    }

    private async Task CreateIndexAsync(IServiceProvider serviceProvider)
    {
        var types = GetTypesAssignableFrom<IIndexBuild>(typeof(PortkeyIndexerCAModule).Assembly);
        var elasticIndexService = serviceProvider.GetRequiredService<IElasticIndexService>();
        foreach (var t in types)
        {
            var indexName = $"{ClientId}-{Version}.{t.Name}".ToLower();
            await elasticIndexService.CreateIndexAsync(indexName, t);
        }
    }

    private List<Type> GetTypesAssignableFrom<T>(Assembly assembly)
    {
        var compareType = typeof(T);
        return assembly.DefinedTypes
            .Where(type => compareType.IsAssignableFrom(type) && !compareType.IsAssignableFrom(type.BaseType) &&
                           !type.IsAbstract && type.IsClass && compareType != type)
            .Cast<Type>().ToList();
    }

    private async Task DeleteIndexAsync(IServiceProvider serviceProvider)
    {
        var elasticIndexService = serviceProvider.GetRequiredService<IElasticIndexService>();
        var types = GetTypesAssignableFrom<IIndexBuild>(typeof(PortkeyIndexerCAModule).Assembly);

        foreach (var t in types)
        {
            var indexName = $"{ClientId}-{Version}.{t.Name}".ToLower();
            await elasticIndexService.DeleteIndexAsync(indexName);
        }
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        AsyncHelper.RunSync(async () =>
            await DeleteIndexAsync(context.ServiceProvider)
        );
    }
}