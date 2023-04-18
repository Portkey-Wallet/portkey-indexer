using AElf.Contracts.MultiToken;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class TokenTransferredLogEventProcessor:  CAHolderTokenBalanceProcessorBase<Transferred>
{
    public TokenTransferredLogEventProcessor(ILogger<TokenTransferredLogEventProcessor> logger,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<NFTProtocolInfoIndex, LogEventInfo> nftProtocolInfoRepository,
        IAElfIndexerClientEntityRepository<CAHolderSearchTokenNFTIndex, LogEventInfo> caHolderSearchTokenNFTRepository,
        IAElfIndexerClientEntityRepository<CAHolderTokenBalanceIndex, LogEventInfo>
            caHolderTokenBalanceIndexRepository,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> tokenInfoIndexRepository,
        IObjectMapper objectMapper) : base(logger, contractInfoOptions,
        caHolderIndexRepository, nftProtocolInfoRepository, caHolderSearchTokenNFTRepository,
        caHolderTokenBalanceIndexRepository, tokenInfoIndexRepository, objectMapper)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).TokenContractAddress;
    }

    protected override async Task HandleEventAsync(Transferred eventValue, LogEventContext context)
    {
        var from = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.From.ToBase58()),context.ChainId);
        if (from != null)
        {
            await ModifyBalanceAsync(from.CAAddress, eventValue.Symbol, -eventValue.Amount, context);
        }
        var to = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.To.ToBase58()),context.ChainId);
        if (to == null) return;
        await ModifyBalanceAsync(to.CAAddress, eventValue.Symbol, eventValue.Amount, context);
    }
}