using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;
using Guardian = Portkey.Contracts.CA.Guardian;

namespace Portkey.Indexer.CA.Processors;

public class GuardianAddedProcessor : GuardianProcessorBase<GuardianAdded>
{
    public GuardianAddedProcessor(ILogger<GuardianAddedProcessor> logger,
        IObjectMapper objectMapper, IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> repository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<GuardianChangeRecordIndex, TransactionInfo> changeRecordRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo> caHolderTransactionIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo> caHolderTransactionAddressIndexRepository,
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions) : base(
        logger, objectMapper, repository,
        contractInfoOptions, changeRecordRepository, caHolderTransactionIndexRepository,
        caHolderTransactionAddressIndexRepository, caHolderTransactionInfoOptions)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).CAContractAddress;
    }

    protected override async Task HandleEventAsync(GuardianAdded eventValue, LogEventContext context)
    {
        await HandlerTransactionIndexAsync(eventValue, context);
        //check ca address if already exist in caHolderIndex
        var id = IdGenerateHelper.GetId(context.ChainId, eventValue.CaAddress.ToBase58());
        var caHolderIndex = await Repository.GetFromBlockStateSetAsync(id, context.ChainId);
        // skip accelerate addGuardian
        if (caHolderIndex == null || caHolderIndex.Guardians == null) return;

        var guardian = caHolderIndex.Guardians.FirstOrDefault(g =>
            g.IdentifierHash == eventValue.GuardianAdded_.IdentifierHash.ToHex() &&
            g.VerifierId == eventValue.GuardianAdded_.VerifierId.ToHex() &&
            g.Type == (int)eventValue.GuardianAdded_.Type);

        if (guardian != null) return;

        var guardianAdded = ObjectMapper.Map<Guardian, Entities.Guardian>(eventValue.GuardianAdded_);
        guardianAdded.TransactionId = context.TransactionId;
        caHolderIndex.Guardians.Add(guardianAdded);

        ObjectMapper.Map(context, caHolderIndex);
        await Repository.AddOrUpdateAsync(caHolderIndex);

        await AddChangeRecordAsync(eventValue.CaAddress.ToBase58(), eventValue.CaHash.ToHex(),
            nameof(GuardianAdded), guardianAdded, context);
    }
    
    protected override async Task HandlerTransactionIndexAsync(GuardianAdded eventValue, LogEventContext context)
    {
        await ProcessCAHolderTransactionAsync(context, eventValue.CaAddress.ToBase58());;
    }
}