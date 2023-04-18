using Nest;

namespace Portkey.Indexer.CA.Entities;

public class GuardianAccount
{
    public Guardian Guardian { get; set; }
    [Keyword] public string Value { get; set; }
}