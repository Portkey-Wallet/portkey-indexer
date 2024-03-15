using Google.Protobuf.Collections;
using Newtonsoft.Json;
using Portkey.Indexer.CA.Entities;

namespace Portkey.Indexer.CA;

public class NftExternalInfoHelper
{
    public static NftExternalInfo BuildNftExternalInfo(MapField<string, string> externalInfo)
    {
        var nftExternalInfo = new NftExternalInfo();

        var externalDictionary = externalInfo.Where(t => !t.Key.IsNullOrWhiteSpace())
            .ToDictionary(item => item.Key, item => item.Value);
        nftExternalInfo.ExternalInfoDictionary = externalDictionary;

        if (externalInfo.TryGetValue("__nft_image_url", out var imageUrl))
        {
            nftExternalInfo.ImageUrl = imageUrl;
        }
        else if (externalInfo.TryGetValue("inscription_image", out var inscriptionImage))
        {
            nftExternalInfo.ImageUrl = inscriptionImage;
        }
        else if (externalInfo.TryGetValue("__nft_image_uri", out var inscriptionImageUrl))
        {
            nftExternalInfo.ImageUrl = inscriptionImageUrl;
        }

        var inscriptionDeployMap = new Dictionary<string, string>();
        var inscriptionDeploy404Exists = externalInfo.TryGetValue("__inscription_deploy", out var inscriptionDeploy);
        var inscriptionDeployExists = externalInfo.TryGetValue("inscription_deploy", out var inscriptionDeployInfo);
        if (inscriptionDeploy404Exists)
        {
            inscriptionDeployMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(inscriptionDeploy);
        }
        else if (inscriptionDeployExists)
        {
            inscriptionDeployMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(inscriptionDeployInfo);
        }

        if (inscriptionDeployMap.TryGetValue("tick", out var inscriptionName))
        {
            nftExternalInfo.InscriptionName = inscriptionName;
        }

        if (inscriptionDeployMap.TryGetValue("lim", out var lim))
        {
            nftExternalInfo.Lim = lim;
        }


        if (externalInfo.TryGetValue("__seed_owned_symbol", out var seedOwnedSymbol))
        {
            nftExternalInfo.SeedOwnedSymbol = seedOwnedSymbol;
        }

        if (externalInfo.TryGetValue("__seed_exp_time", out var seedExpTime))
        {
            nftExternalInfo.Expires = seedExpTime;
        }

        if (externalInfo.TryGetValue("__inscription_adopt", out var inscriptionAdopt))
        {
            var inscriptionAdoptMap =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(inscriptionAdopt);
            if (inscriptionAdoptMap.TryGetValue("gen", out var gen))
            {
                nftExternalInfo.Generation = gen;
            }

            if (inscriptionAdoptMap.TryGetValue("tick", out var tick))
            {
                nftExternalInfo.InscriptionName = tick;
            }
        }

        if (externalInfo.TryGetValue("__nft_attributes", out var attributes))
        {
            nftExternalInfo.Traits = attributes;
        }

        return nftExternalInfo;
    }
}