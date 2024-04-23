using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class ManagerSocialRecoveredProcessor : CAHolderTransactionProcessorBase<ManagerInfoSocialRecovered>
{
    public ManagerSocialRecoveredProcessor(ILogger<ManagerSocialRecoveredProcessor> logger,
        IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderManagerIndex, TransactionInfo> caHolderManagerIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>
            caHolderTransactionIndexRepository,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, TransactionInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, TransactionInfo> nftInfoIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo> caHolderTransactionAddressIndexRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions, IObjectMapper objectMapper) :
        base(logger, caHolderIndexRepository,caHolderManagerIndexRepository, caHolderTransactionIndexRepository, tokenInfoIndexRepository,
            nftInfoIndexRepository, caHolderTransactionAddressIndexRepository, contractInfoOptions, caHolderTransactionInfoOptions, objectMapper)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).CAContractAddress;
    }
    
    protected override async Task HandleEventAsync(ManagerInfoSocialRecovered eventValue, LogEventContext context)
    {
        await HandlerTransactionIndexAsync(eventValue, context);
        //check manager is already exist in caHolderManagerIndex
        var managerIndexId = IdGenerateHelper.GetId(context.ChainId, eventValue.Manager.ToBase58());
        var caHolderManagerIndex =
            await CAHolderManagerIndexRepository.GetFromBlockStateSetAsync(managerIndexId, context.ChainId);
        if (caHolderManagerIndex == null)
        {
            caHolderManagerIndex = new CAHolderManagerIndex
            {
                Id = managerIndexId,
                Manager = eventValue.Manager.ToBase58(),
                CAAddresses = new List<string>()
                {
                    eventValue.CaAddress.ToBase58()
                }
            };
        }
        else
        {
            if (!caHolderManagerIndex.CAAddresses.Contains(eventValue.CaAddress.ToBase58()))
            {
                caHolderManagerIndex.CAAddresses.Add(eventValue.CaAddress.ToBase58());
            }
        }
        ObjectMapper.Map(context, caHolderManagerIndex);
        await CAHolderManagerIndexRepository.AddOrUpdateAsync(caHolderManagerIndex);
        
        //check ca address if already exist in caHolderIndex
        var indexId = IdGenerateHelper.GetId(context.ChainId, eventValue.CaAddress.ToBase58());
        var caHolderIndex = await CAHolderIndexRepository.GetFromBlockStateSetAsync(indexId,context.ChainId);
        if (caHolderIndex == null)
        {
            return;
        }
        ObjectMapper.Map(context, caHolderIndex);
        if (caHolderIndex.ManagerInfos.Count(m => m.Address == eventValue.Manager.ToBase58()) == 0)
        {
            caHolderIndex.ManagerInfos.Add(new Entities.ManagerInfo()
            {
                Address = eventValue.Manager.ToBase58(),
                ExtraData = eventValue.ExtraData
            });
        }

        await CAHolderIndexRepository.AddOrUpdateAsync(caHolderIndex);
    }

    protected override async Task HandlerTransactionIndexAsync(ManagerInfoSocialRecovered eventValue, LogEventContext context)
    {
        await ProcessCAHolderTransactionAsync(context, eventValue.CaAddress.ToBase58());
    }
}