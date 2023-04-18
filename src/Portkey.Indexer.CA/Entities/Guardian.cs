using Nest;

namespace Portkey.Indexer.CA.Entities;

public class Guardian
{
    public int Type { get; set; }
    [Keyword]public string Verifier { get; set; }
}