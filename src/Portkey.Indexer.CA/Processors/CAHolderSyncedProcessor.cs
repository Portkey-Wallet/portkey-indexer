using AElf;
using AElf.Types;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;
using Guardian = Portkey.Indexer.CA.Entities.Guardian;
using ManagerInfo = Portkey.Indexer.CA.Entities.ManagerInfo;

namespace Portkey.Indexer.CA.Processors;

public class CAHolderSyncedProcessor : AElfLogEventProcessorBase<CAHolderSynced, LogEventInfo>
{
    private readonly IObjectMapper _objectMapper;
    private readonly IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> _caHolderIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<CAHolderManagerIndex, LogEventInfo>
        _caHolderManagerIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<LoginGuardianIndex, LogEventInfo>
        _loginGuardianRepository;

    private readonly ContractInfoOptions _contractInfoOptions;

    public CAHolderSyncedProcessor(ILogger<CAHolderSyncedProcessor> logger, IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderManagerIndex, LogEventInfo> caHolderManagerIndexRepository,
        IAElfIndexerClientEntityRepository<LoginGuardianIndex, LogEventInfo> loginGuardianRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base(logger)
    {
        _objectMapper = objectMapper;
        _caHolderIndexRepository = caHolderIndexRepository;
        _caHolderManagerIndexRepository = caHolderManagerIndexRepository;
        _loginGuardianRepository = loginGuardianRepository;
        _contractInfoOptions = contractInfoOptions.Value;
    }

    public override string GetContractAddress(string chainId)
    {
        return _contractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).CAContractAddress;
    }

    protected override async Task HandleEventAsync(CAHolderSynced eventValue, LogEventContext context)
    {
        //check ca address if already exist in caHolderIndex
        var caHolderIndexId = IdGenerateHelper.GetId(context.ChainId, eventValue.CaAddress.ToBase58());
        var caHolderIndex = await _caHolderIndexRepository.GetFromBlockStateSetAsync(caHolderIndexId, context.ChainId);
        if (caHolderIndex == null)
        {
            //CaHolder Create
            await CreateCAHolderAysnc(eventValue, context);
        }
        else
        {
            //Add or Remove manager
            await AddOrRemoveManager(caHolderIndex, eventValue, context);
        }

        //Add LoginGuardians
        await AddLoginGuardians(eventValue, context);

        //Unbound LoginGuardians
        await UnboundLoginGuardians(eventValue, context);
    }

    private async Task CreateCAHolderAysnc(CAHolderSynced eventValue,
        LogEventContext context)
    {
        var managerList = new List<ManagerInfo>();
        if (eventValue.ManagerInfosAdded.ManagerInfos.Count > 0)
        {
            foreach (var item in eventValue.ManagerInfosAdded.ManagerInfos)
            {
                //check manager is already exist in caHolderManagerIndex
                var managerIndexId = IdGenerateHelper.GetId(context.ChainId, item.Address.ToBase58());
                var caHolderManagerIndex =
                    await _caHolderManagerIndexRepository.GetFromBlockStateSetAsync(managerIndexId, context.ChainId);
                if (caHolderManagerIndex == null)
                {
                    caHolderManagerIndex = new CAHolderManagerIndex
                    {
                        Id = managerIndexId,
                        Manager = item.Address.ToBase58(),
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

                _objectMapper.Map<LogEventContext, CAHolderManagerIndex>(context, caHolderManagerIndex);
                await _caHolderManagerIndexRepository.AddOrUpdateAsync(caHolderManagerIndex);

                //add manager info to manager list
                managerList.Add(new ManagerInfo
                {
                    Address = item.Address.ToBase58(),
                    ExtraData = item.ExtraData
                });
            }
        }

        var caHolderIndex = new CAHolderIndex
        {
            Id = IdGenerateHelper.GetId(context.ChainId, eventValue.CaAddress.ToBase58()),
            CAHash = eventValue.CaHash.ToHex(),
            CAAddress = eventValue.CaAddress.ToBase58(),
            Creator = eventValue.Creator.ToBase58(),
            ManagerInfos = managerList
        };

        if (eventValue.CreateChainId == 0)
        {
            var originChainId = await GetOriginChainIdAsync(eventValue.CaHash.ToHex());
            caHolderIndex.OriginChainId = originChainId;
        }
        else
        {
            caHolderIndex.OriginChainId = ChainHelper.ConvertChainIdToBase58(eventValue.CreateChainId);
        }

        _objectMapper.Map<LogEventContext, CAHolderIndex>(context, caHolderIndex);
        await _caHolderIndexRepository.AddOrUpdateAsync(caHolderIndex);
    }

    private async Task AddOrRemoveManager(CAHolderIndex caHolderIndex, CAHolderSynced eventValue,
        LogEventContext context)
    {
        //Add manager
        if (eventValue.ManagerInfosAdded.ManagerInfos.Count > 0)
        {
            foreach (var item in eventValue.ManagerInfosAdded.ManagerInfos)
            {
                if (caHolderIndex.ManagerInfos.Count(m =>
                        m.Address == item.Address.ToBase58() && m.ExtraData == item.ExtraData) == 0)
                {
                    caHolderIndex.ManagerInfos.Add(new ManagerInfo
                    {
                        Address = item.Address.ToBase58(),
                        ExtraData = item.ExtraData
                    });
                }

                //check manager is already exist in caHolderManagerIndex
                var managerIndexId = IdGenerateHelper.GetId(context.ChainId, item.Address.ToBase58());
                var caHolderManagerIndex =
                    await _caHolderManagerIndexRepository.GetFromBlockStateSetAsync(managerIndexId, context.ChainId);
                if (caHolderManagerIndex == null)
                {
                    caHolderManagerIndex = new CAHolderManagerIndex
                    {
                        Id = managerIndexId,
                        Manager = item.Address.ToBase58(),
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

                _objectMapper.Map<LogEventContext, CAHolderManagerIndex>(context, caHolderManagerIndex);
                await _caHolderManagerIndexRepository.AddOrUpdateAsync(caHolderManagerIndex);
            }
        }

        // TODO When deploy new CA contract, remove this part
        var managerInfosRemoved = eventValue.ManagerInfosRemoved.ManagerInfos;

        //Remove manager
        if (managerInfosRemoved.Count > 0)
        {
            foreach (var item in managerInfosRemoved)
            {
                var removeItem = caHolderIndex.ManagerInfos.FirstOrDefault(m =>
                    m.Address == item.Address.ToBase58() && m.ExtraData == item.ExtraData);
                if (removeItem != null)
                {
                    caHolderIndex.ManagerInfos.Remove(removeItem);
                }

                //check manager is already exist in caHolderManagerIndex
                var managerIndexId = IdGenerateHelper.GetId(context.ChainId, item.Address.ToBase58());
                var caHolderManagerIndex =
                    await _caHolderManagerIndexRepository.GetFromBlockStateSetAsync(managerIndexId, context.ChainId);
                if (caHolderManagerIndex != null)
                {
                    if (caHolderManagerIndex.CAAddresses.Contains(eventValue.CaAddress.ToBase58()))
                    {
                        caHolderManagerIndex.CAAddresses.Remove(eventValue.CaAddress.ToBase58());
                        _objectMapper.Map<LogEventContext, CAHolderManagerIndex>(context, caHolderManagerIndex);
                    }

                    if (caHolderManagerIndex.CAAddresses.Count == 0)
                    {
                        await _caHolderManagerIndexRepository.DeleteAsync(caHolderManagerIndex);
                    }
                    else
                    {
                        await _caHolderManagerIndexRepository.AddOrUpdateAsync(caHolderManagerIndex);
                    }
                }
            }
        }

        if (caHolderIndex.OriginChainId.IsNullOrWhiteSpace())
        {
            if (eventValue.CreateChainId == 0)
            {
                var originChainId = await GetOriginChainIdAsync(eventValue.CaHash.ToHex());
                caHolderIndex.OriginChainId = originChainId;
            }
            else
            {
                caHolderIndex.OriginChainId = ChainHelper.ConvertChainIdToBase58(eventValue.CreateChainId);
            }
        }

        _objectMapper.Map<LogEventContext, CAHolderIndex>(context, caHolderIndex);
        await _caHolderIndexRepository.AddOrUpdateAsync(caHolderIndex);
    }

    private async Task AddLoginGuardians(CAHolderSynced eventValue,
        LogEventContext context)
    {
        if (eventValue.LoginGuardiansAdded.LoginGuardians.Count > 0)
        {
            foreach (var item in eventValue.LoginGuardiansAdded.LoginGuardians)
            {
                var indexId = IdGenerateHelper.GetId(context.ChainId, eventValue.CaAddress.ToBase58(),
                    item, Hash.Empty.ToHex());
                var loginGuardianIndex =
                    await _loginGuardianRepository.GetFromBlockStateSetAsync(indexId, context.ChainId);
                if (loginGuardianIndex != null)
                {
                    continue;
                }

                loginGuardianIndex = new LoginGuardianIndex
                {
                    Id = indexId,
                    CAHash = eventValue.CaHash.ToHex(),
                    CAAddress = eventValue.CaAddress.ToBase58(),
                    // Manager = eventValue.Manager.ToBase58(),
                    LoginGuardian = new Guardian
                    {
                        // Guardian = new Entities.Guardian
                        // {
                        //     Type = (int)eventValue.LoginGuardian.Guardian.Type,
                        //     Verifier = eventValue.LoginGuardian.Guardian.Verifier.Id.ToHex()
                        // },
                        IdentifierHash = item.ToHex(),
                        IsLoginGuardian = true
                    }
                };
                _objectMapper.Map(context, loginGuardianIndex);
                await _loginGuardianRepository.AddOrUpdateAsync(loginGuardianIndex);
            }
        }
    }

    private async Task UnboundLoginGuardians(CAHolderSynced eventValue,
        LogEventContext context)
    {
        if (eventValue.LoginGuardiansUnbound.LoginGuardians.Count > 0)
        {
            foreach (var item in eventValue.LoginGuardiansUnbound.LoginGuardians)
            {
                var indexId = IdGenerateHelper.GetId(context.ChainId, eventValue.CaAddress.ToBase58(),
                    item, Hash.Empty.ToHex());
                var loginGuardianIndex = await _loginGuardianRepository.GetAsync(indexId);
                if (loginGuardianIndex == null)
                {
                    continue;
                }

                _objectMapper.Map(context, loginGuardianIndex);
                await _loginGuardianRepository.DeleteAsync(loginGuardianIndex);
            }
        }
    }

    private async Task<string> GetOriginChainIdAsync(string caHash)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Term(i => i.Field(f => f.CAHash).Value(caHash)));
        QueryContainer Filter(QueryContainerDescriptor<CAHolderIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await _caHolderIndexRepository.GetListAsync(Filter);

        return result.Item2.FirstOrDefault(t => t != null && !t.OriginChainId.IsNullOrWhiteSpace())?.OriginChainId;
    }
}