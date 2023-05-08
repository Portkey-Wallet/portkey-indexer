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
    private readonly IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> _caHolderIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<CAHolderManagerIndex, LogEventInfo>
        _caHolderManagerIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<CAHolderManagerChangeRecordIndex, LogEventInfo>
        _caHolderManagerChangeRecordIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, LogEventInfo>
        _caHolderTransactionAddressIndexRepository;

    private readonly IObjectMapper _objectMapper;

    public ManagerLogEventProcessorTests()
    {
        _caHolderIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo>>();
        _caHolderManagerIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderManagerIndex, LogEventInfo>>();
        _caHolderManagerChangeRecordIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderManagerChangeRecordIndex, LogEventInfo>>();
        _caHolderTransactionAddressIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, LogEventInfo>>();
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
        var managerAddedLogEventProcessor = GetRequiredService<ManagerAddedLogEventProcessor>();
        var managerAddedProcessor = GetRequiredService<ManagerAddedProcessor>();

        //step1: create blockStateSet
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash
        };

        var blockStateSet2 = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash
        };

        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);
        var blockStateSetKey2 = await InitializeBlockStateSetAsync(blockStateSet2, chainId);

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
        await managerAddedProcessor.HandleEventAsync(logEventInfo, logEventContext);
        managerAddedProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey2);
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
        var managerAddedLogEventProcessor = GetRequiredService<ManagerAddedLogEventProcessor>();
        var managerAddedProcessor = GetRequiredService<ManagerAddedProcessor>();

        //step1: create blockStateSet
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        var blockStateSet2 = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey2 = await InitializeBlockStateSetAsync(blockStateSet2, chainId);

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
        await managerAddedProcessor.HandleEventAsync(logEventInfo, logEventContext);
        managerAddedProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey2);
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
        var managerAddedLogEventProcessor = GetRequiredService<ManagerAddedLogEventProcessor>();
        var managerAddedProcessor = GetRequiredService<ManagerAddedProcessor>();

        //step1: create blockStateSet
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        var blockStateSet2 = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey2 = await InitializeBlockStateSetAsync(blockStateSet2, chainId);

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
        await managerAddedProcessor.HandleEventAsync(logEventInfo, logEventContext);
        managerAddedProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey2);
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
        var managerRemovedLogEventProcessor = GetRequiredService<ManagerRemovedLogEventProcessor>();
        var managerRemovedProcessor = GetRequiredService<ManagerRemovedProcessor>();

        //step1: create blockStateSet
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        var blockStateSet2 = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey2 = await InitializeBlockStateSetAsync(blockStateSet2, chainId);

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
        await managerRemovedProcessor.HandleEventAsync(logEventInfo, logEventContext);
        managerRemovedProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey2);
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
        var managerRemovedLogEventProcessor = GetRequiredService<ManagerRemovedLogEventProcessor>();
        var managerRemovedProcessor = GetRequiredService<ManagerRemovedProcessor>();

        //step1: create blockStateSet
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        var blockStateSet2 = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey2 = await InitializeBlockStateSetAsync(blockStateSet2, chainId);

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
        await managerRemovedProcessor.HandleEventAsync(logEventInfo, logEventContext);
        managerRemovedProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey2);
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
        var managerSocialRecoveredLogEventProcessor = GetRequiredService<ManagerSocialRecoveredLogEventProcessor>();
        var managerSocialRecoveredProcessor = GetRequiredService<ManagerSocialRecoveredProcessor>();

        //step1: create blockStateSet
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        var blockStateSet2 = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey2 = await InitializeBlockStateSetAsync(blockStateSet2, chainId);

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
        await managerSocialRecoveredLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        managerSocialRecoveredLogEventProcessor.GetContractAddress("AELF");
        await managerSocialRecoveredProcessor.HandleEventAsync(logEventInfo, logEventContext);
        managerSocialRecoveredProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey2);
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
        var managerSocialRecoveredLogEventProcessor = GetRequiredService<ManagerSocialRecoveredLogEventProcessor>();

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
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
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
        var managerSocialRecoveredLogEventProcessor = GetRequiredService<ManagerSocialRecoveredLogEventProcessor>();

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
        await managerSocialRecoveredLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        managerSocialRecoveredLogEventProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
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
        var managerUpdatedLogEventProcessor = GetRequiredService<ManagerUpdatedLogEventProcessor>();
        var managerUpdatedProcessor = GetRequiredService<ManagerUpdatedProcessor>();

        //step1: create blockStateSet
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        var blockStateSet2 = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey2 = await InitializeBlockStateSetAsync(blockStateSet2, chainId);

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
        await managerUpdatedProcessor.HandleEventAsync(logEventInfo, logEventContext);
        managerUpdatedProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey2);
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
        var managerUpdatedLogEventProcessor = GetRequiredService<ManagerUpdatedLogEventProcessor>();

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
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await Task.Delay(2000);

        //step5: check result
        var caHolderManagerIndexData =
            await _caHolderManagerIndexRepository.GetAsync(chainId + "-" + managerInfoUpdated.Manager.ToBase58());
        caHolderManagerIndexData.ShouldBeNull();
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
                    new ()
                    {
                        CAAddress = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
                        ChainId = "AELF"
                    }
                },
                SkipCount = 0,
                MaxResultCount = 10
            });
        result.TotalRecordCount.ShouldBe(1);
        result.Data.Count.ShouldBe(1);
        result.Data.First().CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        result.Data.First().ChainId.ShouldBe("AELF");
        result.Data.First().AddressChainId.ShouldBe("AELF");
        result.Data.First().Address.ShouldBe(Address.FromPublicKey("DDD".HexToByteArray()).ToBase58());
        result.Data.First().TransactionTime.ShouldNotBe(0);
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
}