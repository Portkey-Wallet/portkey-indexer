using AElf;
using AElf.CSharp.Core.Extension;
using AElf.Types;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Nethereum.Hex.HexConvertors.Extensions;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Portkey.Indexer.CA.GraphQL;
using Portkey.Indexer.CA.Tests.Helper;
using Portkey.Indexer.CA.Processors;
using Portkey.Indexer.Orleans.TestBase;
using Shouldly;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace Portkey.Indexer.CA.Tests.Processors;

[Collection(ClusterCollection.Name)]
public sealed class CAHolderAccelerateCreationProcessorTest : PortkeyIndexerCATestBase
{
    private readonly IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> _caHolderIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<CAHolderManagerIndex, LogEventInfo>
        _caHolderManagerIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<LoginGuardianIndex, LogEventInfo> _loginGuardianRepository;
    private readonly IObjectMapper _objectMapper;


    public CAHolderAccelerateCreationProcessorTest()
    {
        _caHolderIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo>>();
        _caHolderManagerIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderManagerIndex, LogEventInfo>>();
        _loginGuardianRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<LoginGuardianIndex, LogEventInfo>>();
        _objectMapper = GetRequiredService<IObjectMapper>();
    }

    [Fact]
    public async Task HandleEventAsync_Test()
    {
        //step1: create blockStateSet
        const string chainId = "AELF";
        const string blockHash = "7043b2f76fef1923357a0857085c038fda34b968de7215d4c64e02aa4a4f41ec";
        const string previousBlockHash = "36c94b6bf009dd11f5d7ca6aadf00d9cdb6806fef37a9d146f188d944a1fd57f";
        const string transactionId = "af8c23caebe62e34d3847799f121d973b514ef0d987905325552bb4da4e53753";
        const long blockHeight = 100;
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetTransaction = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };

        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);
        var blockStateSetKeyTransaction = await InitializeBlockStateSetAsync(blockStateSetTransaction, chainId);

        //step2: create logEventInfo
        var preCrossChainSyncHolderInfoCreated = new PreCrossChainSyncHolderInfoCreated
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            Creator = Address.FromPublicKey("BBB".HexToByteArray()),
            Manager = Address.FromPublicKey("CCC".HexToByteArray()),
            ExtraData = "ExtraData",
            CreateChainId = ChainHelper.ConvertBase58ToChainId("tDVV")
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(preCrossChainSyncHolderInfoCreated.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId = chainId;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        var logEventContext = new LogEventContext
        {
            ChainId = chainId,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId,
            Params = "{ \"to\": \"ca\", \"symbol\": \"ELF\", \"amount\": \"100000000000\" }",
            To = "CAAddress",
            MethodName = "CreateCAHolderOnNonCreateChain",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };

        //step3: handle event and write result to blockStateSet
        var caHolderAccelerateCreationLogEventProcessor =
            GetRequiredService<CAHolderAccelerateCreationLogEventProcessor>();
        await caHolderAccelerateCreationLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        caHolderAccelerateCreationLogEventProcessor.GetContractAddress(chainId);

        var caHolderAccelerateCreationProcessor = GetRequiredService<CAHolderAccelerateCreationProcessor>();
        await caHolderAccelerateCreationProcessor.HandleEventAsync(logEventInfo, logEventContext);
        caHolderAccelerateCreationProcessor.GetContractAddress(chainId);

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        //step5: check result
        var caHolderIndexData = await _caHolderIndexRepository.GetAsync(
            $"{chainId}-{preCrossChainSyncHolderInfoCreated.CaAddress.ToBase58()}");
        caHolderIndexData.BlockHeight.ShouldBe(blockHeight);
        caHolderIndexData.ManagerInfos.FirstOrDefault().Address
            .ShouldBe(preCrossChainSyncHolderInfoCreated.Manager.ToBase58());
        var caHolderManagerIndexData = await _caHolderManagerIndexRepository.GetAsync(
            $"{chainId}-{preCrossChainSyncHolderInfoCreated.Manager.ToBase58()}");
        caHolderManagerIndexData.BlockHeight.ShouldBe(blockHeight);
        caHolderManagerIndexData.CAAddresses.FirstOrDefault()
            .ShouldBe(preCrossChainSyncHolderInfoCreated.CaAddress.ToBase58());
    }

    [Fact]
    public async Task Query_CAHolderInfo_Test()
    {
        await HandleEventAsync_Test();
        await Task.Delay(1000);
        var param = new GetCAHolderInfoDto()
        {
            CAHash = HashHelper.ComputeFrom("test@google.com").ToHex()
        };
        var result = await Query.CAHolderInfo(_caHolderIndexRepository, _loginGuardianRepository, _objectMapper, param);
        result.Count.ShouldBe(1);
        result.FirstOrDefault().CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
    }
}