using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class LoginGuardianAccountChangeRecordIndex : LoginGuardianAccountBase, IIndexBuild
{
    [Keyword] public string ChangeType { get; set; }

}