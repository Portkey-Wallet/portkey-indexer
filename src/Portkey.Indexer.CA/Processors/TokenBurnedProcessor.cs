using AElf.Contracts.MultiToken;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Portkey.Indexer.CA.Provider;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class TokenBurnedProcessor:  CAHolderTokenBalanceProcessorBase<Burned>
{
    public TokenBurnedProcessor(ILogger<TokenBurnedProcessor> logger,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, TransactionInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTCollectionInfoIndex, TransactionInfo> nftCollectionInfoRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, TransactionInfo> nftInfoRepository,
        IAElfIndexerClientEntityRepository<CAHolderSearchTokenNFTIndex, TransactionInfo> caHolderSearchTokenNFTRepository,
        IAElfIndexerClientEntityRepository<CAHolderTokenBalanceIndex, TransactionInfo>
            caHolderTokenBalanceIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderNFTCollectionBalanceIndex, TransactionInfo> caHolderNFTCollectionBalanceIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderNFTBalanceIndex, TransactionInfo> caHolderNFTBalanceIndexRepository,
        IAElfDataProvider aelfDataProvider,
        IObjectMapper objectMapper,
        IOptionsSnapshot<InscriptionListOptions> inscriptionListOptions) : base(logger, contractInfoOptions,
        caHolderIndexRepository, tokenInfoIndexRepository,nftCollectionInfoRepository,nftInfoRepository, caHolderSearchTokenNFTRepository,
        caHolderTokenBalanceIndexRepository,caHolderNFTCollectionBalanceIndexRepository, caHolderNFTBalanceIndexRepository, aelfDataProvider, 
        objectMapper, inscriptionListOptions)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).TokenContractAddress;
    }

    protected override async Task HandleEventAsync(Burned eventValue, LogEventContext context)
    {
        await UpdateTokenSupply(eventValue, context);
        var holder = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.Burner.ToBase58()),context.ChainId);
        if (holder == null) return;
        await ModifyBalanceAsync(holder.CAAddress, eventValue.Symbol, -eventValue.Amount, context);
    }
    
    private async Task UpdateTokenSupply(Burned eventValue, LogEventContext context)
    {
        TokenType tokenType = TokenHelper.GetTokenType(eventValue.Symbol);

        if (tokenType == TokenType.Token)
        {
            var id = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol);
            var tokenInfoIndex = await TokenInfoIndexRepository.GetFromBlockStateSetAsync(id,context.ChainId);
            if (tokenInfoIndex != null)
            {
                tokenInfoIndex.Supply -= eventValue.Amount;
                ObjectMapper.Map(context, tokenInfoIndex);
                await TokenInfoIndexRepository.AddOrUpdateAsync(tokenInfoIndex);
            }
        }

        if (tokenType == TokenType.NFTCollection)
        {
            var id = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol);
            var nftCollectionInfoIndex = await NftCollectionInfoRepository.GetFromBlockStateSetAsync(id,context.ChainId);
            if (nftCollectionInfoIndex != null)
            {
                nftCollectionInfoIndex.Supply -= eventValue.Amount;
                ObjectMapper.Map(context, nftCollectionInfoIndex);
                await NftCollectionInfoRepository.AddOrUpdateAsync(nftCollectionInfoIndex);
            }
        }

        if (tokenType == TokenType.NFTItem)
        {
            var id = IdGenerateHelper.GetId(context.ChainId, eventValue.Symbol);
            var nftInfoIndex = await NftInfoRepository.GetFromBlockStateSetAsync(id,context.ChainId);
            if (nftInfoIndex != null)
            {
                nftInfoIndex.Supply -= eventValue.Amount;
                ObjectMapper.Map(context, nftInfoIndex);
                await NftInfoRepository.AddOrUpdateAsync(nftInfoIndex);
            }
        }
        
    }
}