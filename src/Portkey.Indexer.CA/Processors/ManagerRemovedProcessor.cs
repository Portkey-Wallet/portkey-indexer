using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class ManagerRemovedProcessor: CAHolderManagerProcessorBase<ManagerInfoRemoved>
{
    public ManagerRemovedProcessor(ILogger<ManagerRemovedProcessor> logger,
        IObjectMapper objectMapper, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> repository,
        IAElfIndexerClientEntityRepository<CAHolderManagerIndex, TransactionInfo> caHolderManagerIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderManagerChangeRecordIndex, TransactionInfo> changeRecordRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo> caHolderTransactionIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo> caHolderTransactionAddressIndexRepository,
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions) :
        base(logger, objectMapper, contractInfoOptions, repository,caHolderManagerIndexRepository, changeRecordRepository,
            caHolderTransactionIndexRepository, caHolderTransactionAddressIndexRepository, caHolderTransactionInfoOptions)
    {
    }
    
    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).CAContractAddress;
    }
    
    protected override async Task HandleEventAsync(ManagerInfoRemoved eventValue, LogEventContext context)
    {
        await HandlerTransactionIndexAsync(eventValue, context);
        //check manager is already exist in caHolderManagerIndex
        var managerIndexId = IdGenerateHelper.GetId(context.ChainId, eventValue.Manager.ToBase58());
        var caHolderManagerIndex =
            await CAHolderManagerIndexRepository.GetFromBlockStateSetAsync(managerIndexId, context.ChainId);
        if (caHolderManagerIndex != null)
        {
            if (caHolderManagerIndex.CAAddresses.Contains(eventValue.CaAddress.ToBase58()))
            {
                caHolderManagerIndex.CAAddresses.Remove(eventValue.CaAddress.ToBase58());
                ObjectMapper.Map<LogEventContext, CAHolderManagerIndex>(context, caHolderManagerIndex);
            }

            if (caHolderManagerIndex.CAAddresses.Count == 0)
            {
                await CAHolderManagerIndexRepository.DeleteAsync(caHolderManagerIndex);
            }
            else
            {
                await CAHolderManagerIndexRepository.AddOrUpdateAsync(caHolderManagerIndex);
            }
        }
        
        //check ca address if already exist in caHolderIndex
        var indexId = IdGenerateHelper.GetId(context.ChainId, eventValue.CaAddress.ToBase58());
        var caHolderIndex = await Repository.GetFromBlockStateSetAsync(indexId,context.ChainId);
        if (caHolderIndex == null)
        {
            return;
        }
        ObjectMapper.Map(context, caHolderIndex);
        
        var item = caHolderIndex.ManagerInfos.FirstOrDefault(m => m.Address == eventValue.Manager.ToBase58());
        if (item != null)
        {
            caHolderIndex.ManagerInfos.Remove(item);
        }
        
        await Repository.AddOrUpdateAsync(caHolderIndex);
        await AddChangeRecordAsync(eventValue.CaAddress.ToBase58(), eventValue.CaHash.ToHex(),
            eventValue.Manager.ToBase58(), nameof(ManagerInfoRemoved), context);
    }
    
    protected override async Task HandlerTransactionIndexAsync(ManagerInfoRemoved eventValue, LogEventContext context)
    {
        await ProcessCAHolderTransactionAsync(context, eventValue.CaAddress.ToBase58());;
    }
}