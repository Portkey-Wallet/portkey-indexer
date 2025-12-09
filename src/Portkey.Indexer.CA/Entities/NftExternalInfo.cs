namespace Portkey.Indexer.CA.Entities;

public class NftExternalInfo
{
    public string Traits { get; set; }

    public string Lim { get; set; }

    public string InscriptionName { get; set; }

    public string Generation { get; set; }

    public string SeedOwnedSymbol { get; set; }

    public string Expires { get; set; }
    
    public string ImageUrl { get; set; }
    
    public Dictionary<string,string> ExternalInfoDictionary { get; set; }
}