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
using Portkey.Indexer.CA.Processors;
using Portkey.Indexer.CA.Tests.Helper;
using Shouldly;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace Portkey.Indexer.CA.Tests.Processors;

public class GuardianProcessorTests : PortkeyIndexerCATestBase
{
    private readonly IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> _caHolderIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, LogEventInfo>
        _caHolderTransactionRepository;

    private readonly IAElfIndexerClientEntityRepository<GuardianChangeRecordIndex, LogEventInfo>
        _changeRecordRepository;

    private readonly IObjectMapper _objectMapper;

    public GuardianProcessorTests()
    {
        _caHolderIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo>>();
        _caHolderTransactionRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, LogEventInfo>>();

        _objectMapper = GetRequiredService<IObjectMapper>();
        _changeRecordRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<GuardianChangeRecordIndex, LogEventInfo>>();
    }

    [Fact]
    public async Task HandleGuardianAddedLogEventAsync_Test()
    {
        await CreateHolder();

        //step1: create blockStateSet
        const string chainId = "AELF";
        const string blockHash = "3c7c267341e9f097b0886c8a1661bef73d6bb4c30464ad73be714fdf22b09bdd";
        const string previousBlockHash = "9a6ef475e4c4b6f15c37559033bcfdbed34ca666c67b2ae6be22751a3ae171de";
        const string transactionId = "c09b8c142dd5e07acbc1028e5f59adca5b5be93a0680eb3609b773044a852c43";
        const long blockHeight = 200;
        var blockStateSetAdded = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash
        };

        var blockStateSetTransaction = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash
        };

        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSetAdded, chainId);
        var blockStateSetKeyTransaction = await InitializeBlockStateSetAsync(blockStateSetTransaction, chainId);

        //step2: create logEventInfo
        var guardianAdded = new GuardianAdded
        {
            CaHash = HashHelper.ComputeFrom("syb@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            GuardianAdded_ = new Portkey.Contracts.CA.Guardian()
            {
                IdentifierHash = HashHelper.ComputeFrom("yangtze.cn"),
                Type = GuardianType.OfEmail,
                VerifierId = HashHelper.ComputeFrom("university")
            }
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(guardianAdded.ToLogEvent());
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
            MethodName = "AddGuardian",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };

        //step3: handle event and write result to blockStateSet
        var guardianAddedLogEventProcessor = GetRequiredService<GuardianAddedLogEventProcessor>();
        var guardianAddedProcessor = GetRequiredService<GuardianAddedProcessor>();

        await guardianAddedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await guardianAddedProcessor.HandleEventAsync(logEventInfo, logEventContext);

        guardianAddedLogEventProcessor.GetContractAddress(chainId);
        guardianAddedProcessor.GetContractAddress(chainId);

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        //step5: check result
        var caHolderIndexData =
            await _caHolderIndexRepository.GetAsync(IdGenerateHelper.GetId(chainId,
                guardianAdded.CaAddress.ToBase58()));
        caHolderIndexData.Guardians.Count.ShouldBe(1);
        caHolderIndexData.Guardians.FirstOrDefault().IsLoginGuardian.ShouldBeFalse();
        caHolderIndexData.Guardians.FirstOrDefault().IdentifierHash
            .ShouldBe(HashHelper.ComputeFrom("yangtze.cn").ToHex());
        caHolderIndexData.Guardians.FirstOrDefault().VerifierId.ShouldBe(HashHelper.ComputeFrom("university").ToHex());
        caHolderIndexData.Guardians.FirstOrDefault().Type.ShouldBe((int)GuardianType.OfEmail);
    }

    [Fact]
    public async Task HandleGuardianRemovedLogEventAsync_Test()
    {
        await HandleGuardianAddedLogEventAsync_Test();

        //step1: create blockStateSet
        const string chainId = "AELF";
        const string blockHash = "fd67a41d951c98b5364a0bd21de95b1ea11b56d834bd9f570b1a927223be394f";
        const string previousBlockHash = "4e6c59d7a24c48b1355f7a80cb2ccb9276f75877b1e6f22c2c3406f4b907bec8";
        const string transactionId = "7a55da4d8f33975645e3a55ec1edcd79be5832deb87d63b55838e67d231719ee";
        const long blockHeight = 210;
        var blockStateSetAdded = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash
        };

        var blockStateSetTransaction = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash
        };

        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSetAdded, chainId);
        var blockStateSetKeyTransaction = await InitializeBlockStateSetAsync(blockStateSetTransaction, chainId);

        //step2: create logEventInfo
        var guardianRemoved = new GuardianRemoved
        {
            CaHash = HashHelper.ComputeFrom("syb@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            GuardianRemoved_ = new Portkey.Contracts.CA.Guardian()
            {
                IdentifierHash = HashHelper.ComputeFrom("yangtze.cn"),
                Type = GuardianType.OfEmail,
                VerifierId = HashHelper.ComputeFrom("university")
            }
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(guardianRemoved.ToLogEvent());
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
            MethodName = "RemoveGuardian",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };

        //step3: handle event and write result to blockStateSet
        var guardianRemovedLogEventProcessor = GetRequiredService<GuardianRemovedLogEventProcessor>();
        var guardianRemovedProcessor = GetRequiredService<GuardianRemovedProcessor>();

        await guardianRemovedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await guardianRemovedProcessor.HandleEventAsync(logEventInfo, logEventContext);

        guardianRemovedLogEventProcessor.GetContractAddress(chainId);
        guardianRemovedProcessor.GetContractAddress(chainId);

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        //step5: check result
        var caHolderIndexData =
            await _caHolderIndexRepository.GetAsync(IdGenerateHelper.GetId(chainId,
                guardianRemoved.CaAddress.ToBase58()));
        caHolderIndexData.Guardians.Count.ShouldBe(0);
    }

    [Fact]
    public async Task HandleGuardianUpdatedLogEventAsync_Test()
    {
        await HandleGuardianAddedLogEventAsync_Test();

        //step1: create blockStateSet
        const string chainId = "AELF";
        const string blockHash = "13dc2be6a8518e4a0d7b4316742001efdd9ec001001788a40a741d773bf6638b";
        const string previousBlockHash = "fd67a41d951c98b5364a0bd21de95b1ea11b56d834bd9f570b1a927223be394f";
        const string transactionId = "af3032220296a6c3464fbae4df56aa92fedd4a3f58b06e3ec858401820787749";
        const long blockHeight = 220;
        var blockStateSetAdded = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash
        };
        var blockStateSetTransaction = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash
        };

        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSetAdded, chainId);
        var blockStateSetKeyTransaction = await InitializeBlockStateSetAsync(blockStateSetTransaction, chainId);

        //step2: create logEventInfo
        var guardianUpdated = new GuardianUpdated
        {
            CaHash = HashHelper.ComputeFrom("syb@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            GuardianUpdatedPre = new Portkey.Contracts.CA.Guardian()
            {
                IdentifierHash = HashHelper.ComputeFrom("yangtze.cn"),
                Type = GuardianType.OfEmail,
                VerifierId = HashHelper.ComputeFrom("university")
            },
            GuardianUpdatedNew = new Portkey.Contracts.CA.Guardian()
            {
                IdentifierHash = HashHelper.ComputeFrom("yangtze.cn"),
                Type = GuardianType.OfEmail,
                VerifierId = HashHelper.ComputeFrom("online")
            }
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(guardianUpdated.ToLogEvent());
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
            MethodName = "UpdateGuardian",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };

        //step3: handle event and write result to blockStateSet
        var guardianUpdatedLogEventProcessor = GetRequiredService<GuardianUpdatedLogEventProcessor>();
        var guardianUpdatedProcessor = GetRequiredService<GuardianUpdatedProcessor>();

        await guardianUpdatedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await guardianUpdatedProcessor.HandleEventAsync(logEventInfo, logEventContext);

        guardianUpdatedLogEventProcessor.GetContractAddress(chainId);
        guardianUpdatedProcessor.GetContractAddress(chainId);

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        //step5: check result
        var caHolderIndexData =
            await _caHolderIndexRepository.GetAsync(IdGenerateHelper.GetId(chainId,
                guardianUpdated.CaAddress.ToBase58()));
        caHolderIndexData.Guardians.Count.ShouldBe(1);
        caHolderIndexData.Guardians.FirstOrDefault().IdentifierHash
            .ShouldBe(HashHelper.ComputeFrom("yangtze.cn").ToHex());
        caHolderIndexData.Guardians.First().VerifierId.ShouldBe(HashHelper.ComputeFrom("online").ToHex());
    }


    private async Task CreateHolder()
    {
        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;

        var caHolderCreatedProcessor = GetRequiredService<CAHolderCreatedLogEventProcessor>();

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
        var caHolderCreated = new CAHolderCreated
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            Creator = Address.FromPublicKey("BBB".HexToByteArray()),
            Manager = Address.FromPublicKey("CCC".HexToByteArray()),
            ExtraData = "ExtraData"
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(caHolderCreated.ToLogEvent());
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
            MethodName = "CreatHolderInfo",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };

        //step3: handle event and write result to blockStateSet
        await caHolderCreatedProcessor.HandleEventAsync(logEventInfo, logEventContext);

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await Task.Delay(2000);
    }

    [Fact]
    public async Task Query_GuardianAddedCAHolderInfo_Test()
    {
        await HandleGuardianAddedLogEventAsync_Test();
        await Task.Delay(1000);
        var param = new GetGuardianAddedCAHolderInfo()
        {
            LoginGuardianIdentifierHash = HashHelper.ComputeFrom("yangtze.cn").ToHex()
        };
        var result = await Query.GuardianAddedCAHolderInfo(_caHolderIndexRepository, _objectMapper, param);
        result.TotalRecordCount.ShouldBe(1);
        result.Data.FirstOrDefault().CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
    }

    [Fact]
    public async Task Query_GuardianChangeRecordInfo_Test()
    {
        await HandleGuardianAddedLogEventAsync_Test();
        await Task.Delay(1000);
        var param = new GetGuardianChangeRecordDto()
        {
            ChainId = "AELF",
            StartBlockHeight = 0,
            EndBlockHeight = 200
        };
        var result = await Query.GuardianChangeRecordInfo(_changeRecordRepository, _objectMapper, param);
        result.Count.ShouldBe(1);
        result.FirstOrDefault().CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
    }
}