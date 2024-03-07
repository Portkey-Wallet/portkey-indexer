using AElf.Indexing.Elasticsearch;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class NFTCollectionInfoIndex : TokenInfoBase, IIndexBuild
{
    [Keyword] public string ImageUrl { get; set; }
    [Keyword] public string InscriptionName { get; set; }
    public int LimitPerMint { get; set; }
    
}