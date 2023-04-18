using AElf;
using AElf.CSharp.Core.Extension;
using AElf.Types;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Nethereum.Hex.HexConvertors.Extensions;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Portkey.Indexer.CA.Processors;
using Portkey.Indexer.CA.Tests.Helper;
using Shouldly;
using Volo.Abp.ObjectMapping;
using Xunit;
using ManagerInfo = Portkey.Contracts.CA.ManagerInfo;

namespace Portkey.Indexer.CA.Tests.Processors;

public class CAHolderSyncedProcessorTests:PortkeyIndexerCATestBase
{
    private readonly IObjectMapper _objectMapper;
    private readonly IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> _caHolderIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<CAHolderManagerIndex, LogEventInfo> _caHolderManagerIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<LoginGuardianIndex, LogEventInfo>
        _loginGuardianRepository;

    public CAHolderSyncedProcessorTests()
    {
        _objectMapper = GetRequiredService<IObjectMapper>();
        _caHolderIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo>>();
        _caHolderManagerIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderManagerIndex, LogEventInfo>>();
        _loginGuardianRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<LoginGuardianIndex, LogEventInfo>>();
    }

    [Fact]
    public async Task HandleEventAsync_Test()
    {
        const string chainId = "AELF";
        const string blockHash = "1d29110ef8085744e8bd4ca4ddca9070036d07f4705b79c549b07115ea1f145b";
        const string previousBlockHash = "4d2986852e78f9d84a1f856ffd0d66264edc91e767d463bf08304f91fedb3d9f";
        const string transactionId = "7a4c16a8aa4bb415b1128d060bb3e356ca7bab9ff77be5838a0ce5c4f5b1fe19";
        const long blockHeight = 111;
        
        //step1: create blockStateSet
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);
        
        //step2: create logEventInfo
        var caHolderSynced = new CAHolderSynced
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            Creator = Address.FromPublicKey("BBB".HexToByteArray()),
            ManagerInfosAdded = new ManagerInfoList()
            {
                ManagerInfos =
                {
                    new List<ManagerInfo>()
                    {
                        new ManagerInfo()
                        {
                            Address = Address.FromPublicKey("CCC".HexToByteArray()),
                            ExtraData = "ExtraData_add_manager1"
                        }
                    },
                    new List<ManagerInfo>()
                    {
                        new ManagerInfo()
                        {
                            Address = Address.FromPublicKey("DDD".HexToByteArray()),
                            ExtraData = "ExtraData_add_manager2"
                        }
                    }
                }
            },
            LoginGuardiansAdded = new LoginGuardianList()
            {
                LoginGuardians =
                {
                    new List<Hash>()
                    {
                        HashHelper.ComputeFrom("12345678"),
                        HashHelper.ComputeFrom("87654321"),
                        HashHelper.ComputeFrom("11111111")
                    }
                }
            },
            LoginGuardiansUnbound = new LoginGuardianList()
            {
                LoginGuardians =
                {
                    new List<Hash>()
                    {
                        HashHelper.ComputeFrom("87654321")
                    }
                }
            }
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(caHolderSynced.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId= chainId;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        var logEventContext = new LogEventContext
        {
            ChainId = chainId,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId
        };
        
        //step3: handle event and write result to blockStateSet
        var caHolderSyncedProcessor = GetRequiredService<CAHolderSyncedProcessor>();
        await caHolderSyncedProcessor.HandleEventAsync(logEventInfo, logEventContext);
        caHolderSyncedProcessor.GetContractAddress(chainId);
        
        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await Task.Delay(3000);
        
        //step5: check result
        var caHolderIndexData =
            await _caHolderIndexRepository.GetAsync(
                IdGenerateHelper.GetId(chainId, caHolderSynced.CaAddress.ToBase58()));
        caHolderIndexData.BlockHeight.ShouldBe(blockHeight);
        caHolderIndexData.CAHash.ShouldBe(caHolderSynced.CaHash.ToHex());
        caHolderIndexData.CAAddress.ShouldBe(caHolderSynced.CaAddress.ToBase58());
        caHolderIndexData.Creator.ShouldBe(caHolderSynced.Creator.ToBase58());
        // var removedManagerIndexData = await _caHolderManagerIndexRepository.GetAsync(IdGenerateHelper.GetId(chainId, caHolderSynced.ManagerInfosAdded.ManagerInfos[0].Address.ToBase58()));
        // removedManagerIndexData.ShouldBeNull();
        var caHolderManagerIndexData = await _caHolderManagerIndexRepository.GetAsync(IdGenerateHelper.GetId(chainId,caHolderSynced.ManagerInfosAdded.ManagerInfos[1].Address.ToBase58()));
        caHolderManagerIndexData.BlockHeight.ShouldBe(blockHeight);
        caHolderManagerIndexData.Manager.ShouldBe(caHolderSynced.ManagerInfosAdded.ManagerInfos[1].Address.ToBase58());
        var loginGuardianIndexData =
            await _loginGuardianRepository.GetAsync(IdGenerateHelper.GetId(chainId, caHolderSynced.CaAddress.ToBase58(),
                Hash.Empty.ToHex()));
        loginGuardianIndexData.ShouldBeNull();
    }
    
    [Fact]
    public async Task HandleEventAsync_ManagerInfosAddedRemoved_Test()
    {
        await HandleEventAsync_Test();
        
        const string chainId = "AELF";
        const string blockHash = "3570093141989859be8db4254f2f34604dddfaf23552095d468ea84e2401dc4e";
        const string previousBlockHash = "409470cfd0b24bda969aeef36de1d8da869fc447da0b28b479094ab25910fb32";
        const string transactionId = "fd8939d8d03ba8b005ad632bb7d6983b4714f3e6494e54488c59237e6634b8d8";
        const long blockHeight = 113;
        
        //step1: create blockStateSet
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);
        
        //step2: create logEventInfo
        var caHolderSynced = new CAHolderSynced
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            Creator = Address.FromPublicKey("BBB".HexToByteArray()),
            ManagerInfosAdded = new ManagerInfoList()
            {
                ManagerInfos =
                {
                    new List<ManagerInfo>()
                    {
                        new ManagerInfo()
                        {
                            Address = Address.FromPublicKey("EEE".HexToByteArray()),
                            ExtraData = "ExtraData_add_manager3"
                        }
                    },
                    new List<ManagerInfo>()
                    {
                        new ManagerInfo()
                        {
                            Address = Address.FromPublicKey("FFF".HexToByteArray()),
                            ExtraData = "ExtraData_add_manager4"
                        }
                    }
                }
            },
            ManagerInfosRemoved = new ManagerInfoList()
            {
                ManagerInfosRemoved =
                {
                    new List<ManagerInfo>()
                    {
                        new ManagerInfo()
                        {
                            Address = Address.FromPublicKey("CCC".HexToByteArray()),
                            ExtraData = "ExtraData_add_manager1"
                        }
                    }
                }
            }
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(caHolderSynced.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId= chainId;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        var logEventContext = new LogEventContext
        {
            ChainId = chainId,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId
        };
        
        //step3: handle event and write result to blockStateSet
        var caHolderSyncedProcessor = GetRequiredService<CAHolderSyncedProcessor>();
        await caHolderSyncedProcessor.HandleEventAsync(logEventInfo, logEventContext);
        caHolderSyncedProcessor.GetContractAddress(chainId);
        
        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await Task.Delay(3000);
        
        //step5: check result
        var caHolderIndexData =
            await _caHolderIndexRepository.GetAsync(
                IdGenerateHelper.GetId(chainId, caHolderSynced.CaAddress.ToBase58()));
        caHolderIndexData.BlockHeight.ShouldBe(blockHeight);
        caHolderIndexData.CAHash.ShouldBe(caHolderSynced.CaHash.ToHex());
        caHolderIndexData.CAAddress.ShouldBe(caHolderSynced.CaAddress.ToBase58());
        caHolderIndexData.Creator.ShouldBe(caHolderSynced.Creator.ToBase58());
        caHolderIndexData.ManagerInfos.Count.ShouldBe(3);
        // var removedManagerIndexData = await _caHolderManagerIndexRepository.GetAsync(IdGenerateHelper.GetId(chainId, caHolderSynced.ManagerInfosAdded.ManagerInfos[0].Address.ToBase58()));
        // removedManagerIndexData.ShouldBeNull();
        var caHolderManagerIndexData = await _caHolderManagerIndexRepository.GetAsync(IdGenerateHelper.GetId(chainId,caHolderSynced.ManagerInfosAdded.ManagerInfos[1].Address.ToBase58()));
        caHolderManagerIndexData.BlockHeight.ShouldBe(blockHeight);
        caHolderManagerIndexData.Manager.ShouldBe(caHolderSynced.ManagerInfosAdded.ManagerInfos[1].Address.ToBase58());
        var loginGuardianIndexData =
            await _loginGuardianRepository.GetAsync(IdGenerateHelper.GetId(chainId, caHolderSynced.CaAddress.ToBase58(),
                Hash.Empty.ToHex()));
        loginGuardianIndexData.ShouldBeNull();
    }
}