using AElf.Contracts.MultiToken;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class TokenBurnedLogEventProcessor:  CAHolderTokenBalanceProcessorBase<Burned>
{
    public TokenBurnedLogEventProcessor(ILogger<TokenBurnedLogEventProcessor> logger,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<NFTProtocolInfoIndex, LogEventInfo> nftProtocolInfoRepository,
        IAElfIndexerClientEntityRepository<CAHolderSearchTokenNFTIndex, LogEventInfo> caHolderSearchTokenNFTRepository,
        IAElfIndexerClientEntityRepository<CAHolderTokenBalanceIndex, LogEventInfo> caHolderTokenBalanceIndexRepository,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> tokenInfoIndexRepository,
        IObjectMapper objectMapper) : base(logger, contractInfoOptions,
        caHolderIndexRepository, nftProtocolInfoRepository, caHolderSearchTokenNFTRepository,
        caHolderTokenBalanceIndexRepository,
        tokenInfoIndexRepository, objectMapper)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).TokenContractAddress;
    }

    protected override async Task HandleEventAsync(Burned eventValue, LogEventContext context)
    {
        var holder = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.Burner.ToBase58()),context.ChainId);
        if (holder == null) return;
        await ModifyBalanceAsync(holder.CAAddress, eventValue.Symbol, -eventValue.Amount, context);
    }
}