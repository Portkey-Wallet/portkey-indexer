using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class CAHolderSyncedProcessor: AElfLogEventProcessorBase<CAHolderSynced,LogEventInfo>
{
    private readonly IObjectMapper _objectMapper;
    private readonly IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> _caHolderIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<CAHolderManagerIndex, LogEventInfo> _caHolderManagerIndexRepository;
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
        return _contractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).CAContractAddress;
    }

    protected override async Task HandleEventAsync(CAHolderSynced eventValue, LogEventContext context)
    {
        //check ca address if already exist in caHolderIndex
        var caHolderIndexId = IdGenerateHelper.GetId(context.ChainId, eventValue.CaAddress.ToBase58());
        var caHolderIndex = await _caHolderIndexRepository.GetFromBlockStateSetAsync(caHolderIndexId, context.ChainId);
        if (caHolderIndex == null)
        {
            //CaHolder Create
            var managerList = new List<Entities.ManagerInfo>();
            if (eventValue.ManagerInfosAdded.ManagerInfosAdded.Count > 0)
            {
                foreach (var item in eventValue.ManagerInfosAdded.ManagerInfosAdded)
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
                    managerList.Add(new Entities.ManagerInfo
                    {
                        Address = item.Address.ToBase58(),
                        ExtraData = item.ExtraData
                    });
                }
            }
            caHolderIndex = new CAHolderIndex
            {
                Id = caHolderIndexId,
                CAHash = eventValue.CaHash.ToHex(),
                CAAddress = eventValue.CaAddress.ToBase58(),
                Creator = eventValue.Creator.ToBase58(),
                ManagerInfos = managerList
            };
            _objectMapper.Map<LogEventContext, CAHolderIndex>(context, caHolderIndex);
            await _caHolderIndexRepository.AddOrUpdateAsync(caHolderIndex);
        }
        else
        {
            //Add manager
            if (eventValue.ManagerInfosAdded.ManagerInfosAdded.Count > 0)
            {
                foreach (var item in eventValue.ManagerInfosAdded.ManagerInfosAdded)
                {
                    if (caHolderIndex.ManagerInfos.Count(m => m.Address == item.Address.ToBase58()) == 0)
                    {
                        caHolderIndex.ManagerInfos.Add(new Entities.ManagerInfo
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

            //Remove manager
            if (eventValue.ManagerInfosRemoved.ManagerInfosRemoved.Count > 0)
            {
                foreach (var item in eventValue.ManagerInfosRemoved.ManagerInfosRemoved)
                {
                    if (caHolderIndex.ManagerInfos.Count(m => m.Address == item.Address.ToBase58()) > 0)
                    {
                        var removeItem=caHolderIndex.ManagerInfos.FirstOrDefault(m => m.Address == item.Address.ToBase58());
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
            _objectMapper.Map<LogEventContext, CAHolderIndex>(context, caHolderIndex);
            await _caHolderIndexRepository.AddOrUpdateAsync(caHolderIndex);
        }
        
        //Add LoginGuardians
        if (eventValue.LoginGuardiansAdded.LoginGuardiansAdded.Count > 0)
        {
            foreach (var item in eventValue.LoginGuardiansAdded.LoginGuardiansAdded)
            {
                var indexId = IdGenerateHelper.GetId(context.ChainId, eventValue.CaAddress.ToBase58(),
                    item, "0000000000000000000000000000000000000000000000000000000000000000");
                var loginGuardianIndex = await _loginGuardianRepository.GetFromBlockStateSetAsync(indexId, context.ChainId);
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
                    LoginGuardian = new Entities.Guardian
                    {
                        // Guardian = new Entities.Guardian
                        // {
                        //     Type = (int)eventValue.LoginGuardian.Guardian.Type,
                        //     Verifier = eventValue.LoginGuardian.Guardian.Verifier.Id.ToHex()
                        // },
                        IdentifierHash = item.ToHex()
                    }
                };
                _objectMapper.Map(context, loginGuardianIndex);
                await _loginGuardianRepository.AddOrUpdateAsync(loginGuardianIndex);
            }
            
        }
        //Unbound LoginGuardians
        if (eventValue.LoginGuardiansUnbound.LoginGuardiansUnbound.Count > 0)
        {
            foreach (var item in eventValue.LoginGuardiansUnbound.LoginGuardiansUnbound)
            {
                var indexId = IdGenerateHelper.GetId(context.ChainId, eventValue.CaAddress.ToBase58(),
                    item, "0000000000000000000000000000000000000000000000000000000000000000");
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
    
}