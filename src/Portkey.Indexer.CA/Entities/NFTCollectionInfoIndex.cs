using AElf.Indexing.Elasticsearch;
using Castle.Components.DictionaryAdapter;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class NFTCollectionInfoIndex : TokenInfoBase, IIndexBuild
{
    [Text(Index = false)] public string ImageUrl { get; set; }
    [Keyword] public string InscriptionName { get; set; } 
    
    [Keyword] public string Generation { get; set; }
    
    [Keyword] public string Lim { get; set; }
}