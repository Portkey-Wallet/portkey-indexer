using AElf.Indexing.Elasticsearch;
using Castle.Components.DictionaryAdapter;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class NFTCollectionInfoIndex : TokenInfoBase, IIndexBuild
{
    [Keyword] public string ImageUrl { get; set; }
    [Keyword] public string InscriptionName { get; set; }
    public string LimitPerMint { get; set; }
    [Keyword] public string Generation { get; set; }
}