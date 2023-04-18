using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class LoginGuardianAccountBase : AElfIndexerClientEntity<string>
{
    [Keyword] public override string Id { get; set; }

    /// <summary>
    /// CA holder hash(Id)
    /// </summary>
    [Keyword]
    public string CAHash { get; set; }

    [Keyword] public string CAAddress { get; set; }
    
    [Keyword]
    public string Manager { get; set; }

    public GuardianAccount LoginGuardianAccount { get; set; }
}