using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class GuardianUpdatedProcessor : CAHolderTransactionProcessorBase<GuardianUpdated>
{
    public GuardianUpdatedProcessor(ILogger<GuardianUpdatedProcessor> logger,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderManagerIndex, LogEventInfo> caHolderManagerIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>
            caHolderTransactionIndexRepository,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, LogEventInfo> nftInfoIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo>
            caHolderTransactionAddressIndexRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions, IObjectMapper objectMapper) :
        base(logger, caHolderIndexRepository, caHolderManagerIndexRepository, caHolderTransactionIndexRepository,
            tokenInfoIndexRepository,
            nftInfoIndexRepository, caHolderTransactionAddressIndexRepository, contractInfoOptions,
            caHolderTransactionInfoOptions, objectMapper)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).CAContractAddress;
    }

    protected override async Task HandleEventAsync(GuardianUpdated eventValue, LogEventContext context)
    {
        await ProcessCAHolderTransactionAsync(context, eventValue.CaAddress.ToBase58());
    }
}