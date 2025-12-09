using Orleans.TestingHost;
using Portkey.Indexer.TestBase;
using Volo.Abp.Modularity;

namespace Portkey.Indexer.Orleans.TestBase;

public abstract class PortkeyIndexerOrleansTestBase<TStartupModule>:PortkeyIndexerTestBase<TStartupModule> 
    where TStartupModule : IAbpModule
{
    protected readonly TestCluster Cluster;

    public PortkeyIndexerOrleansTestBase()
    {
        Cluster = GetRequiredService<ClusterFixture>().Cluster;
    }
}