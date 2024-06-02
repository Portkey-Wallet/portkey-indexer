using AElf.Contracts.MultiToken;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class TokenUnApprovedProcessor : CAHolderTransactionProcessorBase<UnApproved>
{
    private readonly IAElfIndexerClientEntityRepository<CAHolderTokenApprovedIndex, TransactionInfo>
        _batchApprovedIndexRepository;

    public TokenUnApprovedProcessor(ILogger<TokenUnApprovedProcessor> logger,
        IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderManagerIndex, TransactionInfo> caHolderManagerIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>
            caHolderTransactionIndexRepository,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, TransactionInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, TransactionInfo> nftInfoIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo>
            caHolderTransactionAddressIndexRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions,
        IAElfIndexerClientEntityRepository<CAHolderTokenApprovedIndex, TransactionInfo> batchApprovedIndexRepository,
        IObjectMapper objectMapper) :
        base(logger, caHolderIndexRepository, caHolderManagerIndexRepository, caHolderTransactionIndexRepository,
            tokenInfoIndexRepository,
            nftInfoIndexRepository, caHolderTransactionAddressIndexRepository, contractInfoOptions,
            caHolderTransactionInfoOptions, objectMapper)
    {
        _batchApprovedIndexRepository = batchApprovedIndexRepository;
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).TokenContractAddress;
    }

    protected override async Task HandleEventAsync(UnApproved eventValue, LogEventContext context)
    {
        if (eventValue.Symbol.Equals("*") || (eventValue.Symbol.Contains("-") && !eventValue.Symbol.Contains("-*")))
        {
            return;
        }
        var holder = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.Owner.ToBase58()), context.ChainId);
        if (holder == null) return;
        var batchApprovedIndexId = IdGenerateHelper.GetId(context.ChainId, eventValue.Owner.ToBase58(), eventValue.Spender.ToBase58(), eventValue.Symbol);
        var batchApprovedIndex =
            await _batchApprovedIndexRepository.GetFromBlockStateSetAsync(batchApprovedIndexId, context.ChainId);
        if (batchApprovedIndex == null)
            return;
        batchApprovedIndex.BatchApprovedAmount = Math.Max(0, batchApprovedIndex.BatchApprovedAmount - eventValue.Amount);
        ObjectMapper.Map(context, batchApprovedIndex);
        await _batchApprovedIndexRepository.AddOrUpdateAsync(batchApprovedIndex);
    }
}