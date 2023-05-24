using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class BingoIndex : AElfIndexerClientEntity<string>, IIndexBuild
{
    [Keyword]public long play_block_height { get; set; }
    [Keyword]public long bingo_block_height { get; set; }
    [Keyword]public long amount { get; set; }
    [Keyword]public long award { get; set; }
    [Keyword]public bool is_complete { get; set; }
    [Keyword]public string play_id { get; set; }
    [Keyword]public string bingo_id { get; set; }
    [Keyword]public int bingoType { get; set; }
    [Keyword]public List<int> dices { get; set; }
    [Keyword]public string player_address { get; set; }
    [Keyword]public long playTime { get; set; }
    [Keyword]public long bingoTime { get; set; }
}
