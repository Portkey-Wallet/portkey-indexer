using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class CAHolderIndex : AElfIndexerClientEntity<string>, IIndexBuild
{
    [Keyword]public override string Id { get; set; }
    
    /// <summary>
    /// CA holder hash(Id)
    /// </summary>
    [Keyword]public string CAHash { get; set; }
    
    /// <summary>
    /// CA holder address
    /// </summary>
    [Keyword]public string CAAddress { get; set; }
    
    /// <summary>
    /// CA holder creator address
    /// </summary>
    [Keyword]public string Creator { get; set; }
    
    /// <summary>
    /// CA Holder manager address list
    /// </summary>
    [Nested(Name = "Managers",Enabled = true,IncludeInParent = true,IncludeInRoot = true)]
    public List<ManagerInfo> Managers { get; set; }
}

public class ManagerInfo
{
    [Keyword]public string Manager { get; set; }
    
    [Keyword]public string DeviceString { get; set; }
}

