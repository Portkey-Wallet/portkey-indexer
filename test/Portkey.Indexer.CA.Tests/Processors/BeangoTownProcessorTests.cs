using AElf;
using AElf.CSharp.Core.Extension;
using AElf.Types;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Nethereum.Hex.HexConvertors.Extensions;
using Portkey.Contracts.BeangoTownContract;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Portkey.Indexer.CA.Processors;
using Portkey.Indexer.CA.Tests.Helper;
using Shouldly;
using Volo.Abp.ObjectMapping;
using Xunit;
using Bingoed = Portkey.Contracts.BeangoTownContract.Bingoed;

namespace Portkey.Indexer.CA.Tests.Processors;

public class BeangoTownProcessorTests : PortkeyIndexerCATestBase
{
    private readonly IAElfIndexerClientEntityRepository<BeangoTownIndex, LogEventInfo> _bingoIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> _caHolderIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, LogEventInfo>
        _caHolderTransactionRepository;

    private readonly IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> _repository;
    private readonly IObjectMapper _objectMapper;

    public BeangoTownProcessorTests()
    {
        _bingoIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<BeangoTownIndex, LogEventInfo>>();
        _caHolderIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo>>();
        _caHolderTransactionRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, LogEventInfo>>();
        _repository = GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo>>();
        _objectMapper = GetRequiredService<IObjectMapper>();
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
    public async Task HandleBingoedLogEventAsync_InvaildInput_Test()
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
        var bingoed = new Bingoed
        {
            PlayBlockHeight = blockHeight,
            BingoBlockHeight = blockHeight,
            IsComplete = true,
            PlayId = HashHelper.ComputeFrom("PlayId")
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(bingoed.ToLogEvent());
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
            MethodName = "Bingoed",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };
        var bingoedLogEventProcessor = GetRequiredService<BeangoTownBeanProcessor>();

        await bingoedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);

        bingoedLogEventProcessor.GetContractAddress(chainId);

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        var bingoGameIndexData = await _bingoIndexRepository.GetAsync(HashHelper.ComputeFrom("PlayId").ToHex());
        bingoGameIndexData.ShouldBeNull();
    }
    
    [Fact]
     public async Task HandlePlayedLogEventAsync_InvaildInput_Test()
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
        var bingoed = new Bingoed
        {
            PlayBlockHeight = blockHeight,
            BingoBlockHeight = blockHeight,
            IsComplete = true,
            PlayId = HashHelper.ComputeFrom("PlayId")
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(bingoed.ToLogEvent());
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
            MethodName = "Played",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };
        var bingoedLogEventProcessor = GetRequiredService<BeangoTownGoProcessor>();

        await bingoedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);

        bingoedLogEventProcessor.GetContractAddress(chainId);

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        var bingoGameIndexData = await _bingoIndexRepository.GetAsync(HashHelper.ComputeFrom("PlayId").ToHex());
        bingoGameIndexData.ShouldBeNull();
    }

    [Fact]
    public async Task HandlePlayedLogEventAsync_Test()
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
        var played = new Played
        {
            PlayBlockHeight = blockHeight,
            PlayerAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            PlayId = HashHelper.ComputeFrom("PlayId"),
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(played.ToLogEvent());
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
            MethodName = "Played",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };
        var playedLogEventProcessor = GetRequiredService<BeangoTownGoProcessor>();

        await playedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);

        playedLogEventProcessor.GetContractAddress(chainId);

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        var bingoGameIndexData = await _bingoIndexRepository.GetAsync(HashHelper.ComputeFrom("PlayId").ToHex());
        bingoGameIndexData.ShouldNotBeNull();
        bingoGameIndexData.PlayBlockHeight.ShouldBe(blockHeight);
        bingoGameIndexData.ChainId.ShouldBe(chainId);
    }
    
     [Fact]
    public async Task HandlePlayedLogEventAsync_play_method_Test()
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
        var played = new Played
        {
            PlayBlockHeight = blockHeight,
            PlayerAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            PlayId = HashHelper.ComputeFrom("PlayId"),
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(played.ToLogEvent());
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
            MethodName = "Play",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };
        var playedLogEventProcessor = GetRequiredService<BeangoTownGoProcessor>();

        await playedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);

        playedLogEventProcessor.GetContractAddress(chainId);

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        var bingoGameIndexData = await _bingoIndexRepository.GetAsync(HashHelper.ComputeFrom("PlayId").ToHex());
        bingoGameIndexData.ShouldNotBeNull();
        bingoGameIndexData.PlayBlockHeight.ShouldBe(blockHeight);
        bingoGameIndexData.ChainId.ShouldBe(chainId);
        
        var transationData = await _caHolderTransactionRepository.GetAsync(IdGenerateHelper.GetId(blockHash, transactionId));
        transationData.ShouldNotBeNull();
        transationData.MethodName.ShouldBe("BeanGoTown-Play");
    }
    
    [Fact]
    public async Task HandleBingoedLogEventAsync_noPlayEvent_Test()
    {
        await CreateHolder();
        //await HandlePlayedLogEventAsync_Test();
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
        var bingoed = new Bingoed
        {
            PlayBlockHeight = blockHeight,
            PlayerAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            BingoBlockHeight = blockHeight,
            IsComplete = true,
            PlayId = HashHelper.ComputeFrom("PlayId")
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(bingoed.ToLogEvent());
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
            MethodName = "Bingoed",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };
        var bingoedLogEventProcessor = GetRequiredService<BeangoTownBeanProcessor>();

        await bingoedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);

        bingoedLogEventProcessor.GetContractAddress(chainId);

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        var bingoIndexData = await _bingoIndexRepository.GetAsync(HashHelper.ComputeFrom("PlayId").ToHex());
        bingoIndexData.ShouldBeNull();
        var transationData = await _caHolderTransactionRepository.GetAsync(IdGenerateHelper.GetId(blockHash, transactionId));
        transationData.ShouldNotBeNull();
        transationData.MethodName.ShouldBe("Bingoed");
    }

    [Fact]
    public async Task HandleBingoedLogEventAsync_Test()
    {
        await CreateHolder();
        await HandlePlayedLogEventAsync_Test();
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
        var bingoed = new Bingoed
        {
            PlayBlockHeight = blockHeight,
            PlayerAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            BingoBlockHeight = blockHeight,
            IsComplete = true,
            PlayId = HashHelper.ComputeFrom("PlayId")
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(bingoed.ToLogEvent());
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
            MethodName = "Bingoed",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };
        var bingoedLogEventProcessor = GetRequiredService<BeangoTownBeanProcessor>();

        await bingoedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);

        bingoedLogEventProcessor.GetContractAddress(chainId);

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        var bingoIndexData = await _bingoIndexRepository.GetAsync(HashHelper.ComputeFrom("PlayId").ToHex());
        bingoIndexData.ShouldNotBeNull();
        bingoIndexData.BingoBlockHeight.ShouldBe(blockHeight);
        bingoIndexData.PlayBlockHeight.ShouldBe(blockHeight);
        bingoIndexData.ChainId.ShouldBe(chainId);
        var transationData = await _caHolderTransactionRepository.GetAsync(IdGenerateHelper.GetId(blockHash, transactionId));
        transationData.ShouldNotBeNull();
        transationData.MethodName.ShouldBe("Bingoed");
    }
    
    //this processor is not use 
    public async Task HandleBingoedLogEventAsync_beango_method_Test()
    {
        await CreateHolder();
        await HandlePlayedLogEventAsync_Test();
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
        var bingoed = new Bingoed
        {
            PlayBlockHeight = blockHeight,
            PlayerAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            BingoBlockHeight = blockHeight,
            IsComplete = true,
            PlayId = HashHelper.ComputeFrom("PlayId")
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(bingoed.ToLogEvent());
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
            MethodName = "Bingo",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };
        var bingoedLogEventProcessor = GetRequiredService<BeangoTownBeanProcessor>();

        await bingoedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);

        bingoedLogEventProcessor.GetContractAddress(chainId);

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        var bingoIndexData = await _bingoIndexRepository.GetAsync(HashHelper.ComputeFrom("PlayId").ToHex());
        bingoIndexData.ShouldNotBeNull();
        bingoIndexData.BingoBlockHeight.ShouldBe(blockHeight);
        bingoIndexData.PlayBlockHeight.ShouldBe(blockHeight);
        bingoIndexData.ChainId.ShouldBe(chainId);
        
        var transationData = await _caHolderTransactionRepository.GetAsync(IdGenerateHelper.GetId(blockHash, transactionId));
        transationData.ShouldNotBeNull();
        transationData.MethodName.ShouldBe("BeanGoTown-Bingo");
    }
}
