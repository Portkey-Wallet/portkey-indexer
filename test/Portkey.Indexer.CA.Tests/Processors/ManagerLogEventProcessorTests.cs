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
using Portkey.Indexer.Orleans.TestBase;
using Shouldly;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace Portkey.Indexer.CA.Tests.Processors;

[Collection(ClusterCollection.Name)]
public sealed class ManagerLogEventProcessorTests : PortkeyIndexerCATestBase
{
    private readonly IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> _caHolderIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<CAHolderManagerIndex, TransactionInfo>
        _caHolderManagerIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<CAHolderManagerChangeRecordIndex, TransactionInfo>
        _caHolderManagerChangeRecordIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo>
        _caHolderTransactionAddressIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<ManagerApprovedIndex, TransactionInfo>
        _managerApprovedIndexRepository;

    private readonly IObjectMapper _objectMapper;
    private static Dictionary<string, string> extraProperties = new Dictionary<string, string>
    {
        { "TransactionFee", "{\"ELF\":\"30000000\"}" },
        { "ResourceFee", "{\"ELF\":\"30000000\"}" }
    };

    public ManagerLogEventProcessorTests()
    {
        _caHolderIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo>>();
        _caHolderManagerIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderManagerIndex, TransactionInfo>>();
        _caHolderManagerChangeRecordIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderManagerChangeRecordIndex, TransactionInfo>>();
        _caHolderTransactionAddressIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo>>();
        _managerApprovedIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<ManagerApprovedIndex, TransactionInfo>>();
        _objectMapper = GetRequiredService<IObjectMapper>();
    }

    [Fact]
    public async Task ManagerInfoAddedTests()
    {
        await CreateHolder();

        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;
        var managerAddedLogEventProcessor = GetRequiredService<ManagerAddedProcessor>();

        //step1: create blockStateSet
        var blockStateSet = new BlockStateSet<TransactionInfo>
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

        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);
        var blockStateSetKeyTransaction = await InitializeBlockStateSetAsync(blockStateSetTransaction, chainId);

        //step2: create logEventInfo
        var managerInfoAdded = new ManagerInfoAdded
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            Manager = Address.FromPublicKey("DDD".HexToByteArray()),
            ExtraData = "ExtraData"
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(managerInfoAdded.ToLogEvent());
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
            MethodName = "AddManagerInfo",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };

        //step3: handle event and write result to blockStateSet
        await managerAddedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        managerAddedLogEventProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        //step5: check result
        var caHolderIndexData =
            await _caHolderIndexRepository.GetAsync(chainId + "-" + managerInfoAdded.CaAddress.ToBase58());
        caHolderIndexData.BlockHeight.ShouldBe(blockHeight);
        caHolderIndexData.ManagerInfos.Last().Address.ShouldBe(managerInfoAdded.Manager.ToBase58());
        var caHolderManagerIndexData =
            await _caHolderManagerIndexRepository.GetAsync(chainId + "-" + managerInfoAdded.Manager.ToBase58());
        caHolderManagerIndexData.BlockHeight.ShouldBe(blockHeight);
        caHolderManagerIndexData.CAAddresses.First().ShouldBe(managerInfoAdded.CaAddress.ToBase58());
    }

    [Fact]
    public async Task ManagerInfoAddedTests_DuplicateManager()
    {
        await CreateHolder();

        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;
        var managerAddedLogEventProcessor = GetRequiredService<ManagerAddedProcessor>();

        //step1: create blockStateSet
        var blockStateSet = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        var blockStateSetTransaction = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKeyTransaction = await InitializeBlockStateSetAsync(blockStateSetTransaction, chainId);

        //step2: create logEventInfo
        var managerInfoAdded = new ManagerInfoAdded
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            Manager = Address.FromPublicKey("CCC".HexToByteArray()),
            ExtraData = "ExtraData"
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(managerInfoAdded.ToLogEvent());
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
            MethodName = "AddManagerInfo",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };

        //step3: handle event and write result to blockStateSet
        await managerAddedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        managerAddedLogEventProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        //step5: check result
        var caHolderIndexData =
            await _caHolderIndexRepository.GetAsync(chainId + "-" + managerInfoAdded.CaAddress.ToBase58());
        caHolderIndexData.BlockHeight.ShouldBe(blockHeight);
        caHolderIndexData.ManagerInfos.Last().Address.ShouldBe(managerInfoAdded.Manager.ToBase58());
        var caHolderManagerIndexData =
            await _caHolderManagerIndexRepository.GetAsync(chainId + "-" + managerInfoAdded.Manager.ToBase58());
        caHolderManagerIndexData.BlockHeight.ShouldBe(blockHeight);
        caHolderManagerIndexData.CAAddresses.First().ShouldBe(managerInfoAdded.CaAddress.ToBase58());
    }

    [Fact]
    public async Task ManagerInfoAddedTests_HolderNotExist()
    {
        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;
        var managerAddedLogEventProcessor = GetRequiredService<ManagerAddedProcessor>();

        //step1: create blockStateSet
        var blockStateSet = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        var blockStateSetTransaction = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKeyTransaction = await InitializeBlockStateSetAsync(blockStateSetTransaction, chainId);

        //step2: create logEventInfo
        var managerInfoAdded = new ManagerInfoAdded
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            Manager = Address.FromPublicKey("CCC".HexToByteArray()),
            ExtraData = "ExtraData"
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(managerInfoAdded.ToLogEvent());
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
            MethodName = "AddManagerInfo",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };

        //step3: handle event and write result to blockStateSet
        await managerAddedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        managerAddedLogEventProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        //step5: check result
        var caHolderManagerIndexData =
            await _caHolderManagerIndexRepository.GetAsync(chainId + "-" + managerInfoAdded.Manager.ToBase58());
        caHolderManagerIndexData.BlockHeight.ShouldBe(blockHeight);
        caHolderManagerIndexData.CAAddresses.First().ShouldBe(managerInfoAdded.CaAddress.ToBase58());
    }

    [Fact]
    public async Task ManagerInfoRemovedTests()
    {
        await ManagerInfoAddedTests();

        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;
        var managerRemovedLogEventProcessor = GetRequiredService<ManagerRemovedProcessor>();

        //step1: create blockStateSet
        var blockStateSet = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        var blockStateSetTransaction = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKeyTransaction = await InitializeBlockStateSetAsync(blockStateSetTransaction, chainId);

        //step2: create logEventInfo
        var managerInfoRemoved = new ManagerInfoRemoved
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            Manager = Address.FromPublicKey("DDD".HexToByteArray()),
            ExtraData = "ExtraData"
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(managerInfoRemoved.ToLogEvent());
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
            MethodName = "RemoveManagerInfo",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };

        //step3: handle event and write result to blockStateSet
        await managerRemovedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        managerRemovedLogEventProcessor.GetContractAddress("AELF");
        
        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        //step5: check result
        var caHolderIndexData =
            await _caHolderIndexRepository.GetAsync(chainId + "-" + managerInfoRemoved.CaAddress.ToBase58());
        caHolderIndexData.BlockHeight.ShouldBe(blockHeight);
        caHolderIndexData.ManagerInfos.Count.ShouldBe(1);
        var caHolderManagerIndexData =
            await _caHolderManagerIndexRepository.GetAsync(chainId + "-" + managerInfoRemoved.Manager.ToBase58());
        caHolderManagerIndexData.ShouldBeNull();
    }

    [Fact]
    public async Task ManagerInfoRemovedTests_HolderNotExist()
    {
        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;
        var managerRemovedLogEventProcessor = GetRequiredService<ManagerRemovedProcessor>();

        //step1: create blockStateSet
        var blockStateSet = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        var blockStateSetTransaction = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKeyTransaction = await InitializeBlockStateSetAsync(blockStateSetTransaction, chainId);

        //step2: create logEventInfo
        var managerInfoRemoved = new ManagerInfoRemoved
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            Manager = Address.FromPublicKey("DDD".HexToByteArray()),
            ExtraData = "ExtraData"
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(managerInfoRemoved.ToLogEvent());
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
            MethodName = "RemoveManagerInfo",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };

        //step3: handle event and write result to blockStateSet
        await managerRemovedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        managerRemovedLogEventProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        //step5: check result
        var caHolderManagerIndexData =
            await _caHolderManagerIndexRepository.GetAsync(chainId + "-" + managerInfoRemoved.Manager.ToBase58());
        caHolderManagerIndexData.ShouldBeNull();
    }

    [Fact]
    public async Task ManagerSocialRecoveredTests()
    {
        await CreateHolder();

        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;
        var managerSocialRecoveredProcessor = GetRequiredService<ManagerSocialRecoveredProcessor>();

        //step1: create blockStateSet
        var blockStateSet = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        var blockStateSetTransaction = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKeyTransaction = await InitializeBlockStateSetAsync(blockStateSetTransaction, chainId);

        //step2: create logEventInfo
        var managerInfoSocialRecovered = new ManagerInfoSocialRecovered
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            Manager = Address.FromPublicKey("DDD".HexToByteArray()),
            ExtraData = "ExtraData"
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(managerInfoSocialRecovered.ToLogEvent());
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
            MethodName = "SocialRecovery",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };

        //step3: handle event and write result to blockStateSet
        await managerSocialRecoveredProcessor.HandleEventAsync(logEventInfo, logEventContext);
        managerSocialRecoveredProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        //step5: check result
        var caHolderIndexData =
            await _caHolderIndexRepository.GetAsync(chainId + "-" + managerInfoSocialRecovered.CaAddress.ToBase58());
        caHolderIndexData.BlockHeight.ShouldBe(blockHeight);
        caHolderIndexData.ManagerInfos.Last().Address.ShouldBe(managerInfoSocialRecovered.Manager.ToBase58());
        var caHolderManagerIndexData =
            await _caHolderManagerIndexRepository.GetAsync(
                chainId + "-" + managerInfoSocialRecovered.Manager.ToBase58());
        caHolderManagerIndexData.BlockHeight.ShouldBe(blockHeight);
        caHolderManagerIndexData.CAAddresses.First().ShouldBe(managerInfoSocialRecovered.CaAddress.ToBase58());
    }

    [Fact]
    public async Task ManagerSocialRecoveredTests_DuplicateManager()
    {
        await CreateHolder();

        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;
        var managerSocialRecoveredLogEventProcessor = GetRequiredService<ManagerSocialRecoveredProcessor>();

        //step1: create blockStateSet
        var blockStateSet = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        //step2: create logEventInfo
        var managerInfoSocialRecovered = new ManagerInfoSocialRecovered
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            Manager = Address.FromPublicKey("CCC".HexToByteArray()),
            ExtraData = "ExtraData"
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(managerInfoSocialRecovered.ToLogEvent());
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
            MethodName = "SocialRecovery",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };

        //step3: handle event and write result to blockStateSet
        await managerSocialRecoveredLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        managerSocialRecoveredLogEventProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await Task.Delay(2000);

        //step5: check result
        var caHolderIndexData =
            await _caHolderIndexRepository.GetAsync(chainId + "-" + managerInfoSocialRecovered.CaAddress.ToBase58());
        caHolderIndexData.BlockHeight.ShouldBe(blockHeight);
        caHolderIndexData.ManagerInfos.Last().Address.ShouldBe(managerInfoSocialRecovered.Manager.ToBase58());
        var caHolderManagerIndexData =
            await _caHolderManagerIndexRepository.GetAsync(
                chainId + "-" + managerInfoSocialRecovered.Manager.ToBase58());
        caHolderManagerIndexData.BlockHeight.ShouldBe(blockHeight);
        caHolderManagerIndexData.CAAddresses.First().ShouldBe(managerInfoSocialRecovered.CaAddress.ToBase58());
    }

    [Fact]
    public async Task ManagerSocialRecoveredTests_HolderNotExist()
    {
        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;
        var managerSocialRecoveredProcessor = GetRequiredService<ManagerSocialRecoveredProcessor>();

        //step1: create blockStateSet
        var blockStateSet = new BlockStateSet<TransactionInfo>
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
        var managerInfoSocialRecovered = new ManagerInfoSocialRecovered
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            Manager = Address.FromPublicKey("DDD".HexToByteArray()),
            ExtraData = "ExtraData"
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(managerInfoSocialRecovered.ToLogEvent());
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
            MethodName = "SocialRecovery",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };

        //step3: handle event and write result to blockStateSet
        await managerSocialRecoveredProcessor.HandleEventAsync(logEventInfo, logEventContext);
        managerSocialRecoveredProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        //step5: check result
        var caHolderManagerIndexData =
            await _caHolderManagerIndexRepository.GetAsync(
                chainId + "-" + managerInfoSocialRecovered.Manager.ToBase58());
        caHolderManagerIndexData.BlockHeight.ShouldBe(blockHeight);
        caHolderManagerIndexData.CAAddresses.First().ShouldBe(managerInfoSocialRecovered.CaAddress.ToBase58());
    }

    [Fact]
    public async Task ManagerUpdatedTests()
    {
        await CreateHolder();

        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;
        var managerUpdatedLogEventProcessor = GetRequiredService<ManagerUpdatedProcessor>();

        //step1: create blockStateSet
        var blockStateSet = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        var blockStateSetTransaction = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKeyTransaction = await InitializeBlockStateSetAsync(blockStateSetTransaction, chainId);

        //step2: create logEventInfo
        var managerInfoUpdated = new ManagerInfoUpdated
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            Manager = Address.FromPublicKey("CCC".HexToByteArray()),
            ExtraData = "new ExtraData"
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(managerInfoUpdated.ToLogEvent());
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
            MethodName = "UpdateManagerInfo",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };

        //step3: handle event and write result to blockStateSet
        await managerUpdatedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        managerUpdatedLogEventProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        //step5: check result
        var caHolderIndexData =
            await _caHolderIndexRepository.GetAsync(chainId + "-" + managerInfoUpdated.CaAddress.ToBase58());
        caHolderIndexData.BlockHeight.ShouldBe(blockHeight);
        caHolderIndexData.ManagerInfos.First().Address.ShouldBe(managerInfoUpdated.Manager.ToBase58());
        caHolderIndexData.ManagerInfos.First().ExtraData.ShouldBe(managerInfoUpdated.ExtraData);
        var caHolderManagerIndexData =
            await _caHolderManagerIndexRepository.GetAsync(chainId + "-" + managerInfoUpdated.Manager.ToBase58());
        caHolderManagerIndexData.BlockHeight.ShouldBe(blockHeight);
        caHolderManagerIndexData.CAAddresses.First().ShouldBe(managerInfoUpdated.CaAddress.ToBase58());
    }

    [Fact]
    public async Task ManagerUpdatedTests_HolderNotExist()
    {
        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;
        var managerUpdatedLogEventProcessor = GetRequiredService<ManagerUpdatedProcessor>();

        //step1: create blockStateSet
        var blockStateSet = new BlockStateSet<TransactionInfo>
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
        var managerInfoUpdated = new ManagerInfoUpdated
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            Manager = Address.FromPublicKey("CCC".HexToByteArray()),
            ExtraData = "new ExtraData"
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(managerInfoUpdated.ToLogEvent());
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
            MethodName = "UpdateManagerInfo",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };

        //step3: handle event and write result to blockStateSet
        await managerUpdatedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        managerUpdatedLogEventProcessor.GetContractAddress("AELF");
        
        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        //step5: check result
        var caHolderManagerIndexData =
            await _caHolderManagerIndexRepository.GetAsync(chainId + "-" + managerInfoUpdated.Manager.ToBase58());
        caHolderManagerIndexData.ShouldBeNull();
    }

    [Fact]
    public async Task ManagerApprovedTests()
    {
        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;
        Hash owner = HashHelper.ComputeFrom("test@google.com");
        Address spenderAddr = Address.FromPublicKey("AAA".HexToByteArray());
        var managerApprovedLogEventProcessor = GetRequiredService<ManagerApprovedProcessor>();
    
        //step1: create blockStateSet
        var blockStateSet = new BlockStateSet<TransactionInfo>
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
        var managerInfoUpdated = new ManagerApproved()
        {
            CaHash = owner,
            Spender = spenderAddr,
            Symbol = "ELF",
            Amount = 10000,
            External = new External()
            {
                Value =
                {
                    new Dictionary<string, string>()
                    {
                        {
                            "testKey", "testValue"
                        }
                    }
                }
            }
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(managerInfoUpdated.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId = chainId;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        var logEventContext = new LogEventContext
        {
            To = "CAAddress",
            ChainId = chainId,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId,
            MethodName = "ManagerApprove",
            BlockTime = DateTime.UtcNow,
            ExtraProperties = extraProperties
        };
    
        //step3: handle event and write result to blockStateSet
        await managerApprovedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        managerApprovedLogEventProcessor.GetContractAddress("AELF");
    
        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);
    
        //step5: check result
        var managerApprovedIndexData = await _managerApprovedIndexRepository.GetAsync(chainId + "-" + transactionId);
        managerApprovedIndexData.ChainId.ShouldBe(chainId);
        managerApprovedIndexData.CaHash.ShouldBe(owner.ToHex());
        managerApprovedIndexData.Spender.ShouldBe(spenderAddr.ToBase58());
        managerApprovedIndexData.Symbol.ShouldBe("ELF");
        managerApprovedIndexData.Amount.ShouldBe(10000);
    }

    [Fact]
    public async Task QueryCAHolderManagerInfoTests()
    {
        await ManagerInfoAddedTests();

        var result = await Query.CAHolderManagerInfo(_caHolderIndexRepository, _objectMapper,
            new GetCAHolderManagerInfoDto
            {
                CAAddresses = new List<string>
                {
                    Address.FromPublicKey("AAA".HexToByteArray()).ToBase58()
                },
                CAHash = HashHelper.ComputeFrom("test@google.com").ToHex(),
                Manager = Address.FromPublicKey("DDD".HexToByteArray()).ToBase58(),
                ChainId = "AELF",
                SkipCount = 0,
                MaxResultCount = 10
            });
        result.First().ChainId.ShouldBe("AELF");
        result.First().CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        result.First().CAHash.ShouldBe(HashHelper.ComputeFrom("test@google.com").ToHex());
        result.First().OriginChainId.ShouldBe("AELF");
        result.First().ManagerInfos.Count.ShouldBe(2);
        result.First().ManagerInfos.Last().Address.ShouldBe(Address.FromPublicKey("DDD".HexToByteArray()).ToBase58());
    }

    [Fact]
    public async Task QueryCAHolderManagerChangeRecordInfoTests()
    {
        await ManagerInfoAddedTests();

        var result = await Query.CAHolderManagerChangeRecordInfo(_caHolderManagerChangeRecordIndexRepository,
            _objectMapper, new GetCAHolderManagerChangeRecordDto
            {
                ChainId = "AELF",
                StartBlockHeight = 0,
                EndBlockHeight = 200
            });
        result.Count.ShouldBe(1);
        result.First().CAHash.ShouldBe(HashHelper.ComputeFrom("test@google.com").ToHex());
        result.First().CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        result.First().Manager.ShouldBe(Address.FromPublicKey("DDD".HexToByteArray()).ToBase58());
        result.First().ChangeType.ShouldBe("ManagerInfoAdded");
        result.First().BlockHeight.ShouldBe(100);
    }

    [Fact]
    public async Task QueryCAHolderTransactionAddressInfoTests()
    {
        await ManagerInfoAddedTests();

        var result = await Query.CAHolderTransactionAddressInfo(_caHolderTransactionAddressIndexRepository,
            _objectMapper, new GetCAHolderTransactionAddressDto
            {
                ChainId = "AELF",
                CAAddressInfos = new List<CAAddressInfo>
                {
                    new()
                    {
                        CAAddress = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
                        ChainId = "AELF"
                    }
                },
                SkipCount = 0,
                MaxResultCount = 10
            });
        result.TotalRecordCount.ShouldBe(0);
        result.Data.Count.ShouldBe(0);
    }

    [Fact]
    public async Task QueryCAHolderManagerApprovedTests()
    {
        await ManagerApprovedTests();
    
        var result = await Query.CAHolderManagerApprovedAsync(_managerApprovedIndexRepository,
            _objectMapper, new GetCAHolderManagerApprovedDto()
            {
                ChainId = "AELF",
                CAHash = HashHelper.ComputeFrom("test@google.com").ToHex(),
                Spender = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
                Symbol = "ELF",
                SkipCount = 0,
                MaxResultCount = 10
            });
        result.TotalRecordCount.ShouldBe(1);
        result.Data.Count.ShouldBe(1);
    }

    private async Task CreateHolder()
    {
        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;

        var caHolderCreatedProcessor = GetRequiredService<CAHolderCreatedProcessor>();

        //step1: create blockStateSet
        var blockStateSet = new BlockStateSet<TransactionInfo>
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
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await Task.Delay(2000);
    }
}