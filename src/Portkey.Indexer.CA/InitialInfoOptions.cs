using Portkey.Indexer.CA.Entities;

namespace Portkey.Indexer.CA;

public class InitialInfoOptions
{
    public List<TokenInfo> TokenInfoList { get; set; } = new();
    public List<NFTProtocolInfo> NFTProtocolInfoList { get; set; } = new ();
}

public class NFTProtocolInfo : NFTProtocolInfoBase
{
    
}

public class TokenInfo : TokenInfoBase
{
    
}