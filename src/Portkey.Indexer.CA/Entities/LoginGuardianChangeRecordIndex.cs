using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class LoginGuardianChangeRecordIndex : LoginGuardianBase, IIndexBuild
{
    [Keyword] public string ChangeType { get; set; }
    
    public bool IsCreateHolder { get; set; }


}