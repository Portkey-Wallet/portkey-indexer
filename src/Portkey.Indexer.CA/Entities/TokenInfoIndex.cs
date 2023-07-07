using AElf.Indexing.Elasticsearch;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class TokenInfoIndex : TokenInfoBase, IIndexBuild
{
    [Wildcard] public string SymbolSearch { get; set; }
    // public TokenInfoIndex RelatedTokenInfo { get; set; }
}

