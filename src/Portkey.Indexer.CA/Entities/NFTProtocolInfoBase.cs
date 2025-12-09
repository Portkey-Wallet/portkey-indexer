using AElfIndexer.Client;
using Nest;

namespace Portkey.Indexer.CA.Entities;

public class NFTProtocolInfoBase : AElfIndexerClientEntity<string>
{
    [Keyword] public override string Id { get; set; }
    
    [Keyword] public string Symbol { get; set; }
    
    [Keyword] public string Creator { get; set; }
    
    [Keyword] public string NftType { get; set; }

    [Keyword] public string ProtocolName { get; set; }
    
    [Keyword] public string BaseUri { get; set; }
    
    public bool IsTokenIdReuse { get; set; }
    
    //TODO Metadata?
    //TODO need to change when NFTMinted and Burned 
    public long Supply { get; set; }
    
    public long TotalSupply { get; set; }
    
    public int IssueChainId { get; set; }
    
    public bool IsBurnable { get; set; }
  
    [Text(Index = false)] public string ImageUrl { get; set; }
}