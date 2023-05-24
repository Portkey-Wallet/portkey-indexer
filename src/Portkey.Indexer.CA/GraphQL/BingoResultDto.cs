using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using GraphQL;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class BingoResultDto
{
    public long TotalRecordCount { get; set; }
    public List<BingoInfo> Data { get; set; }
}

public class BingoInfo 
{
    public long Amount { get; set; }
    public long Award { get; set; }
    public bool IsComplete { get; set; }
    public string PlayId { get; set; }
    public string BingoId { get; set; }
    public int BingoType { get; set; }
    public List<int> Dices { get; set; }
    public string PlayerAddress { get; set; }
    public long PlayTime { get; set; }
    public long BingoTime { get; set; }
}
