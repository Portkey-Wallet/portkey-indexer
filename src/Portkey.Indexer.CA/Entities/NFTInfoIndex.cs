using AElf.Indexing.Elasticsearch;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class NFTInfoIndex : TokenInfoBase, IIndexBuild
{
    [Keyword] public string ImageUrl { get; set; }
    
    [Keyword] public string CollectionSymbol { get; set; }
    
    [Keyword] public string CollectionName { get; set; }
    [Keyword] public string InscriptionName { get; set; }
    public int LimitPerMint { get; set; }
    
    [Keyword] public string SeedOwnedSymbol { get; set; }

    [Keyword] public string Expires { get; set; }

    
    
}