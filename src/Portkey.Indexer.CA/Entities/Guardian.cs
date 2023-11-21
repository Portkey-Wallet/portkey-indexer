using Nest;

namespace Portkey.Indexer.CA.Entities;

public class Guardian
{
    // public Guardian Guardian { get; set; }

    public int Type { get; set; }
    [Keyword] public string VerifierId { get; set; }
    [Keyword] public string IdentifierHash { get; set; }

    [Keyword] public string Salt { get; set; }
    [Keyword] public bool IsLoginGuardian { get; set; }
    [Keyword] public string TransactionId { get; set; }
}