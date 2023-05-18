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
using Guardian = Portkey.Contracts.CA.Guardian;

namespace Portkey.Indexer.CA.Tests.Processors;

[Collection(ClusterCollection.Name)]
public class LoginGuardianProcessorTests : PortkeyIndexerCATestBase
{
    private readonly IAElfIndexerClientEntityRepository<LoginGuardianIndex, LogEventInfo>
        _loginGuardianIndexerRepostory;

    private readonly IAElfIndexerClientEntityRepository<LoginGuardianChangeRecordIndex, LogEventInfo>
        _loginGuardianChangeRecordIndexerClientEntityRepository;

    private readonly IObjectMapper _objectMapper;

    public LoginGuardianProcessorTests()
    {
        _loginGuardianIndexerRepostory =
            GetRequiredService<IAElfIndexerClientEntityRepository<LoginGuardianIndex, LogEventInfo>>();
        _loginGuardianChangeRecordIndexerClientEntityRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<LoginGuardianChangeRecordIndex, LogEventInfo>>();
        _objectMapper = GetRequiredService<IObjectMapper>();
    }

    [Fact]
    public async Task LoginGuardianAddedProcessorTests()
    {
        await CreateHolder();

        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;

        var loginGuardianAddedLogEventProcessor = GetRequiredService<LoginGuardianAddedLogEventProcessor>();
        var loginGuardianAddedProcessor = GetRequiredService<LoginGuardianAddedProcessor>();
        
        //step1: create blockStateSet
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
        var loginGuardianAdded = new LoginGuardianAdded
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            Manager = Address.FromPublicKey("CCC".HexToByteArray()),
            LoginGuardian = new Guardian
            {
                IdentifierHash = HashHelper.ComputeFrom("G"),
                IsLoginGuardian = true,
                Salt = "salt",
                Type = GuardianType.OfEmail,
                VerifierId = HashHelper.ComputeFrom("V")
            }
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(loginGuardianAdded.ToLogEvent());
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
            MethodName = "SetGuardianForLogin",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };

        //step3: handle event and write result to blockStateSet
        await loginGuardianAddedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await loginGuardianAddedProcessor.HandleEventAsync(logEventInfo, logEventContext);
        
        loginGuardianAddedLogEventProcessor.GetContractAddress("AELF");
        loginGuardianAddedProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        //step5: check result
        var loginGuardianIndexData = await _loginGuardianIndexerRepostory.GetAsync(IdGenerateHelper.GetId(chainId,
            Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(), HashHelper.ComputeFrom("G").ToHex(),
            HashHelper.ComputeFrom("V").ToHex()));
        loginGuardianIndexData.LoginGuardian.IsLoginGuardian.ShouldBeTrue();
        loginGuardianIndexData.LoginGuardian.IdentifierHash.ShouldBe(HashHelper.ComputeFrom("G").ToHex());
        loginGuardianIndexData.LoginGuardian.VerifierId.ShouldBe(HashHelper.ComputeFrom("V").ToHex());
        loginGuardianIndexData.LoginGuardian.Salt.ShouldBe("salt");
        loginGuardianIndexData.LoginGuardian.Type.ShouldBe(0);
        loginGuardianIndexData.BlockHeight.ShouldBe(blockHeight);
        loginGuardianIndexData.ChainId.ShouldBe("AELF");
        loginGuardianIndexData.CAHash.ShouldBe(HashHelper.ComputeFrom("test@google.com").ToHex());
        loginGuardianIndexData.CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());

        var loginGuardianChangeRecordIndexData = await _loginGuardianChangeRecordIndexerClientEntityRepository
            .GetAsync(IdGenerateHelper.GetId(chainId, Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
                HashHelper.ComputeFrom("G").ToHex(),
                nameof(LoginGuardianAdded), transactionId));
        loginGuardianChangeRecordIndexData.ChangeType.ShouldBe("LoginGuardianAdded");
        loginGuardianChangeRecordIndexData.BlockHeight.ShouldBe(blockHeight);
        loginGuardianChangeRecordIndexData.ChainId.ShouldBe("AELF");
        loginGuardianChangeRecordIndexData.CAHash.ShouldBe(HashHelper.ComputeFrom("test@google.com").ToHex());
        loginGuardianChangeRecordIndexData.CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        loginGuardianChangeRecordIndexData.LoginGuardian.IdentifierHash.ShouldBe(HashHelper.ComputeFrom("G").ToHex());
    }

    [Fact]
    public async Task LoginGuardianAddedProcessorTests_Guardian()
    {
        await GuardianAdded();

        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;

        var loginGuardianAddedProcessor = GetRequiredService<LoginGuardianAddedLogEventProcessor>();

        //step1: create blockStateSet
        var blockStateSetAdded = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash
        };

        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSetAdded, chainId);

        //step2: create logEventInfo
        var loginGuardianAdded = new LoginGuardianAdded
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            Manager = Address.FromPublicKey("CCC".HexToByteArray()),
            LoginGuardian = new Guardian
            {
                IdentifierHash = HashHelper.ComputeFrom("G"),
                IsLoginGuardian = true,
                Salt = "salt",
                Type = GuardianType.OfEmail,
                VerifierId = HashHelper.ComputeFrom("V")
            }
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(loginGuardianAdded.ToLogEvent());
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
        await loginGuardianAddedProcessor.HandleEventAsync(logEventInfo, logEventContext);
        loginGuardianAddedProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await Task.Delay(2000);

        //step5: check result
        var loginGuardianIndexData = await _loginGuardianIndexerRepostory.GetAsync(IdGenerateHelper.GetId(chainId,
            Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(), HashHelper.ComputeFrom("G").ToHex(),
            HashHelper.ComputeFrom("V").ToHex()));
        loginGuardianIndexData.LoginGuardian.IsLoginGuardian.ShouldBeTrue();
        loginGuardianIndexData.LoginGuardian.IdentifierHash.ShouldBe(HashHelper.ComputeFrom("G").ToHex());
        loginGuardianIndexData.LoginGuardian.VerifierId.ShouldBe(HashHelper.ComputeFrom("V").ToHex());
        loginGuardianIndexData.LoginGuardian.Salt.ShouldBe("salt");
        loginGuardianIndexData.LoginGuardian.Type.ShouldBe(0);
        loginGuardianIndexData.BlockHeight.ShouldBe(blockHeight);
        loginGuardianIndexData.ChainId.ShouldBe("AELF");
        loginGuardianIndexData.CAHash.ShouldBe(HashHelper.ComputeFrom("test@google.com").ToHex());
        loginGuardianIndexData.CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());

        var loginGuardianChangeRecordIndexData = await _loginGuardianChangeRecordIndexerClientEntityRepository
            .GetAsync(IdGenerateHelper.GetId(chainId, Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
                HashHelper.ComputeFrom("G").ToHex(),
                nameof(LoginGuardianAdded), transactionId));
        loginGuardianChangeRecordIndexData.ChangeType.ShouldBe("LoginGuardianAdded");
        loginGuardianChangeRecordIndexData.BlockHeight.ShouldBe(blockHeight);
        loginGuardianChangeRecordIndexData.ChainId.ShouldBe("AELF");
        loginGuardianChangeRecordIndexData.CAHash.ShouldBe(HashHelper.ComputeFrom("test@google.com").ToHex());
        loginGuardianChangeRecordIndexData.CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        loginGuardianChangeRecordIndexData.LoginGuardian.IdentifierHash.ShouldBe(HashHelper.ComputeFrom("G").ToHex());
    }

    [Fact]
    public async Task LoginGuardianAddedProcessorTests_HolderNotExists()
    {
        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;

        var loginGuardianAddedLogEventProcessor = GetRequiredService<LoginGuardianAddedLogEventProcessor>();
        var loginGuardianAddedProcessor = GetRequiredService<LoginGuardianAddedProcessor>();
        
        //step1: create blockStateSet
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
        var loginGuardianAdded = new LoginGuardianAdded
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            Manager = Address.FromPublicKey("CCC".HexToByteArray()),
            LoginGuardian = new Guardian
            {
                IdentifierHash = HashHelper.ComputeFrom("G"),
                IsLoginGuardian = true,
                Salt = "salt",
                Type = GuardianType.OfEmail,
                VerifierId = HashHelper.ComputeFrom("V")
            }
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(loginGuardianAdded.ToLogEvent());
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
            MethodName = "SetGuardianForLogin",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };

        //step3: handle event and write result to blockStateSet
        await loginGuardianAddedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await loginGuardianAddedProcessor.HandleEventAsync(logEventInfo, logEventContext);
        
        loginGuardianAddedLogEventProcessor.GetContractAddress("AELF");
        loginGuardianAddedProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        //step5: check result
        var loginGuardianIndexData = await _loginGuardianIndexerRepostory.GetAsync(IdGenerateHelper.GetId(chainId,
            Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(), HashHelper.ComputeFrom("G").ToHex(),
            HashHelper.ComputeFrom("V").ToHex()));
        loginGuardianIndexData.LoginGuardian.IsLoginGuardian.ShouldBeTrue();
        loginGuardianIndexData.LoginGuardian.IdentifierHash.ShouldBe(HashHelper.ComputeFrom("G").ToHex());
        loginGuardianIndexData.LoginGuardian.VerifierId.ShouldBe(HashHelper.ComputeFrom("V").ToHex());
        loginGuardianIndexData.LoginGuardian.Salt.ShouldBe("salt");
        loginGuardianIndexData.LoginGuardian.Type.ShouldBe(0);
        loginGuardianIndexData.BlockHeight.ShouldBe(blockHeight);
        loginGuardianIndexData.ChainId.ShouldBe("AELF");
        loginGuardianIndexData.CAHash.ShouldBe(HashHelper.ComputeFrom("test@google.com").ToHex());
        loginGuardianIndexData.CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());

        var loginGuardianChangeRecordIndexData = await _loginGuardianChangeRecordIndexerClientEntityRepository
            .GetAsync(IdGenerateHelper.GetId(chainId, Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
                HashHelper.ComputeFrom("G").ToHex(),
                nameof(LoginGuardianAdded), transactionId));
        loginGuardianChangeRecordIndexData.ChangeType.ShouldBe("LoginGuardianAdded");
        loginGuardianChangeRecordIndexData.BlockHeight.ShouldBe(blockHeight);
        loginGuardianChangeRecordIndexData.ChainId.ShouldBe("AELF");
        loginGuardianChangeRecordIndexData.CAHash.ShouldBe(HashHelper.ComputeFrom("test@google.com").ToHex());
        loginGuardianChangeRecordIndexData.CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        loginGuardianChangeRecordIndexData.LoginGuardian.IdentifierHash.ShouldBe(HashHelper.ComputeFrom("G").ToHex());
    }
    
    [Fact]
    public async Task LoginGuardianAddedProcessorTests_LoginGuardianExist()
    {
        await LoginGuardianAddedProcessorTests();

        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;

        var loginGuardianAddedProcessor = GetRequiredService<LoginGuardianAddedLogEventProcessor>();

        //step1: create blockStateSet
        var blockStateSetAdded = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash
        };

        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSetAdded, chainId);

        //step2: create logEventInfo
        var loginGuardianAdded = new LoginGuardianAdded
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            Manager = Address.FromPublicKey("CCC".HexToByteArray()),
            LoginGuardian = new Guardian
            {
                IdentifierHash = HashHelper.ComputeFrom("G"),
                IsLoginGuardian = true,
                Salt = "salt",
                Type = GuardianType.OfEmail,
                VerifierId = HashHelper.ComputeFrom("V")
            }
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(loginGuardianAdded.ToLogEvent());
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
        await loginGuardianAddedProcessor.HandleEventAsync(logEventInfo, logEventContext);
        loginGuardianAddedProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await Task.Delay(2000);

        //step5: check result
        var loginGuardianIndexData = await _loginGuardianIndexerRepostory.GetAsync(IdGenerateHelper.GetId(chainId,
            Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(), HashHelper.ComputeFrom("G").ToHex(),
            HashHelper.ComputeFrom("V").ToHex()));
        loginGuardianIndexData.LoginGuardian.IsLoginGuardian.ShouldBeTrue();
        loginGuardianIndexData.LoginGuardian.IdentifierHash.ShouldBe(HashHelper.ComputeFrom("G").ToHex());
        loginGuardianIndexData.LoginGuardian.VerifierId.ShouldBe(HashHelper.ComputeFrom("V").ToHex());
        loginGuardianIndexData.LoginGuardian.Salt.ShouldBe("salt");
        loginGuardianIndexData.LoginGuardian.Type.ShouldBe(0);
        loginGuardianIndexData.BlockHeight.ShouldBe(blockHeight);
        loginGuardianIndexData.ChainId.ShouldBe("AELF");
        loginGuardianIndexData.CAHash.ShouldBe(HashHelper.ComputeFrom("test@google.com").ToHex());
        loginGuardianIndexData.CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());

        var loginGuardianChangeRecordIndexData = await _loginGuardianChangeRecordIndexerClientEntityRepository
            .GetAsync(IdGenerateHelper.GetId(chainId, Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
                HashHelper.ComputeFrom("G").ToHex(),
                nameof(LoginGuardianAdded), transactionId));
        loginGuardianChangeRecordIndexData.ChangeType.ShouldBe("LoginGuardianAdded");
        loginGuardianChangeRecordIndexData.BlockHeight.ShouldBe(blockHeight);
        loginGuardianChangeRecordIndexData.ChainId.ShouldBe("AELF");
        loginGuardianChangeRecordIndexData.CAHash.ShouldBe(HashHelper.ComputeFrom("test@google.com").ToHex());
        loginGuardianChangeRecordIndexData.CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        loginGuardianChangeRecordIndexData.LoginGuardian.IdentifierHash.ShouldBe(HashHelper.ComputeFrom("G").ToHex());
    }

    [Fact]
    public async Task LoginGuardianRemovedProcessorTests()
    {
        await LoginGuardianAddedProcessorTests();

        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;

        var loginGuardianRemovedLogEventProcessor = GetRequiredService<LoginGuardianRemovedLogEventProcessor>();
        var loginGuardianRemovedProcessor = GetRequiredService<LoginGuardianRemovedProcessor>();

        //step1: create blockStateSet
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
        var loginGuardianRemoved = new LoginGuardianRemoved
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            Manager = Address.FromPublicKey("CCC".HexToByteArray()),
            LoginGuardian = new Guardian
            {
                IdentifierHash = HashHelper.ComputeFrom("G"),
                IsLoginGuardian = false,
                Salt = "salt",
                Type = GuardianType.OfEmail,
                VerifierId = HashHelper.ComputeFrom("V")
            }
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(loginGuardianRemoved.ToLogEvent());
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
            MethodName = "UnsetGuardianForLogin",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };

        //step3: handle event and write result to blockStateSet
        await loginGuardianRemovedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await loginGuardianRemovedProcessor.HandleEventAsync(logEventInfo, logEventContext);
        
        loginGuardianRemovedLogEventProcessor.GetContractAddress("AELF");
        loginGuardianRemovedProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        //step5: check result
        var loginGuardianIndexData = await _loginGuardianIndexerRepostory.GetAsync(IdGenerateHelper.GetId(chainId,
            Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(), HashHelper.ComputeFrom("G").ToHex(),
            HashHelper.ComputeFrom("V").ToHex()));
        loginGuardianIndexData.ShouldBeNull();
    }

    [Fact]
    public async Task LoginGuardianRemovedProcessorTests_LoginGuardianNotExist()
    {
        await CreateHolder();

        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;

        var loginGuardianRemovedProcessor = GetRequiredService<LoginGuardianRemovedLogEventProcessor>();

        //step1: create blockStateSet
        var blockStateSetAdded = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash
        };

        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSetAdded, chainId);

        //step2: create logEventInfo
        var loginGuardianRemoved = new LoginGuardianRemoved
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            Manager = Address.FromPublicKey("CCC".HexToByteArray()),
            LoginGuardian = new Guardian
            {
                IdentifierHash = HashHelper.ComputeFrom("G"),
                IsLoginGuardian = false,
                Salt = "salt",
                Type = GuardianType.OfEmail,
                VerifierId = HashHelper.ComputeFrom("V")
            }
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(loginGuardianRemoved.ToLogEvent());
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
        await loginGuardianRemovedProcessor.HandleEventAsync(logEventInfo, logEventContext);
        loginGuardianRemovedProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await Task.Delay(2000);

        //step5: check result
        var loginGuardianIndexData = await _loginGuardianIndexerRepostory.GetAsync(IdGenerateHelper.GetId(chainId,
            Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(), HashHelper.ComputeFrom("G").ToHex(),
            HashHelper.ComputeFrom("V").ToHex()));
        loginGuardianIndexData.ShouldBeNull();
    }
    
    [Fact]
    public async Task LoginGuardianRemovedProcessorTests_HolderNotExists()
    {
        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;

        var loginGuardianRemovedLogEventProcessor = GetRequiredService<LoginGuardianRemovedLogEventProcessor>();
        var loginGuardianRemovedProcessor = GetRequiredService<LoginGuardianRemovedProcessor>();

        //step1: create blockStateSet
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
        var loginGuardianRemoved = new LoginGuardianRemoved
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            Manager = Address.FromPublicKey("CCC".HexToByteArray()),
            LoginGuardian = new Guardian
            {
                IdentifierHash = HashHelper.ComputeFrom("G"),
                IsLoginGuardian = false,
                Salt = "salt",
                Type = GuardianType.OfEmail,
                VerifierId = HashHelper.ComputeFrom("V")
            }
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(loginGuardianRemoved.ToLogEvent());
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
            MethodName = "UnsetGuardianForLogin",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };

        //step3: handle event and write result to blockStateSet
        await loginGuardianRemovedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await loginGuardianRemovedProcessor.HandleEventAsync(logEventInfo, logEventContext);
        
        loginGuardianRemovedLogEventProcessor.GetContractAddress("AELF");
        loginGuardianRemovedProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        //step5: check result
        var loginGuardianIndexData = await _loginGuardianIndexerRepostory.GetAsync(IdGenerateHelper.GetId(chainId,
            Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(), HashHelper.ComputeFrom("G").ToHex(),
            HashHelper.ComputeFrom("V").ToHex()));
        loginGuardianIndexData.ShouldBeNull();
    }

    [Fact]
    public async Task LoginGuardianUnboundProcessorTests()
    {
        await CreateHolder();

        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;

        var loginGuardianUnboundLogEventProcessor = GetRequiredService<LoginGuardianUnboundLogEventProcessor>();
        var loginGuardianUnboundProcessor = GetRequiredService<LoginGuardianUnboundProcessor>();

        //step1: create blockStateSet
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
        var loginGuardianUnbound = new LoginGuardianUnbound
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            Manager = Address.FromPublicKey("CCC".HexToByteArray()),
            LoginGuardianIdentifierHash = HashHelper.ComputeFrom("G")
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(loginGuardianUnbound.ToLogEvent());
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
            MethodName = "UnsetGuardianForLogin",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };

        //step3: handle event and write result to blockStateSet
        await loginGuardianUnboundLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        loginGuardianUnboundLogEventProcessor.GetContractAddress("AELF");
        
        await loginGuardianUnboundProcessor.HandleEventAsync(logEventInfo, logEventContext);
        loginGuardianUnboundProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        //step5: check result
        var loginGuardianIndexData = await _loginGuardianIndexerRepostory.GetAsync(IdGenerateHelper.GetId(chainId,
            Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(), HashHelper.ComputeFrom("G").ToHex(),
            HashHelper.ComputeFrom("V").ToHex()));
        loginGuardianIndexData.ShouldBeNull();

        var loginGuardianChangeRecordIndexData = await _loginGuardianChangeRecordIndexerClientEntityRepository
            .GetAsync(IdGenerateHelper.GetId(chainId, Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
                HashHelper.ComputeFrom("G").ToHex(),
                nameof(LoginGuardianUnbound), transactionId));
        loginGuardianChangeRecordIndexData.ChangeType.ShouldBe("LoginGuardianUnbound");
        loginGuardianChangeRecordIndexData.BlockHeight.ShouldBe(blockHeight);
        loginGuardianChangeRecordIndexData.ChainId.ShouldBe("AELF");
        loginGuardianChangeRecordIndexData.CAHash.ShouldBe(HashHelper.ComputeFrom("test@google.com").ToHex());
        loginGuardianChangeRecordIndexData.CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        loginGuardianChangeRecordIndexData.LoginGuardian.IdentifierHash.ShouldBe(HashHelper.ComputeFrom("G").ToHex());
    }
    
    [Fact]
    public async Task LoginGuardianUnboundProcessorTests_HolderNotExists()
    {
        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;

        var loginGuardianUnboundLogEventProcessor = GetRequiredService<LoginGuardianUnboundLogEventProcessor>();
        var loginGuardianUnboundProcessor = GetRequiredService<LoginGuardianUnboundProcessor>();

        //step1: create blockStateSet
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
        var loginGuardianUnbound = new LoginGuardianUnbound
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            Manager = Address.FromPublicKey("CCC".HexToByteArray()),
            LoginGuardianIdentifierHash = HashHelper.ComputeFrom("G")
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(loginGuardianUnbound.ToLogEvent());
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
            MethodName = "UnsetGuardianForLogin",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };

        //step3: handle event and write result to blockStateSet
        await loginGuardianUnboundLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        loginGuardianUnboundLogEventProcessor.GetContractAddress("AELF");
        
        await loginGuardianUnboundProcessor.HandleEventAsync(logEventInfo, logEventContext);
        loginGuardianUnboundProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        //step5: check result
        var loginGuardianIndexData = await _loginGuardianIndexerRepostory.GetAsync(IdGenerateHelper.GetId(chainId,
            Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(), HashHelper.ComputeFrom("G").ToHex(),
            HashHelper.ComputeFrom("V").ToHex()));
        loginGuardianIndexData.ShouldBeNull();

        var loginGuardianChangeRecordIndexData = await _loginGuardianChangeRecordIndexerClientEntityRepository
            .GetAsync(IdGenerateHelper.GetId(chainId, Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
                HashHelper.ComputeFrom("G").ToHex(),
                nameof(LoginGuardianUnbound), transactionId));
        loginGuardianChangeRecordIndexData.ChangeType.ShouldBe("LoginGuardianUnbound");
        loginGuardianChangeRecordIndexData.BlockHeight.ShouldBe(blockHeight);
        loginGuardianChangeRecordIndexData.ChainId.ShouldBe("AELF");
        loginGuardianChangeRecordIndexData.CAHash.ShouldBe(HashHelper.ComputeFrom("test@google.com").ToHex());
        loginGuardianChangeRecordIndexData.CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        loginGuardianChangeRecordIndexData.LoginGuardian.IdentifierHash.ShouldBe(HashHelper.ComputeFrom("G").ToHex());
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
    public async Task QueryLoginGuardianInfoTest()
    {
        await LoginGuardianAddedProcessorTests();

        var result = await Query.LoginGuardianInfo(_loginGuardianIndexerRepostory, _objectMapper,
            new GetLoginGuardianInfoDto()
            {
                ChainId = "AELF",
                CAAddress = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
                LoginGuardian = HashHelper.ComputeFrom("G").ToHex()
            });
        result.Count.ShouldBe(1);
        result.FirstOrDefault().CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        result.FirstOrDefault().LoginGuardian.IdentifierHash.ShouldBe(HashHelper.ComputeFrom("G").ToHex());
    }

    [Fact]
    public async Task Query_LoginGuardianChangeRecordInfo_Test()
    {
        await CreateHolder();
        await LoginGuardianAddedProcessorTests();
        await Task.Delay(1000);
        var param = new GetLoginGuardianChangeRecordDto()
        {
            ChainId = "AELF",
            StartBlockHeight = 1,
            EndBlockHeight = 100,
        };
        var result =
            await Query.LoginGuardianChangeRecordInfo(_loginGuardianChangeRecordIndexerClientEntityRepository,
                _objectMapper, param);
        result.FirstOrDefault().ChangeType.ShouldBe("LoginGuardianAdded");
        result.Count.ShouldBe(1);
        result.FirstOrDefault().ChainId.ShouldBe("AELF");
        result.FirstOrDefault().BlockHeight.ShouldBe(100);
    }

    private async Task GuardianAdded()
    {
        await CreateHolder();

        //step1: create blockStateSet
        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;
        var blockStateSetAdded = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash
        };

        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSetAdded, chainId);

        //step2: create logEventInfo
        var guardianAdded = new GuardianAdded
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            CaAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            GuardianAdded_ = new Guardian
            {
                IdentifierHash = HashHelper.ComputeFrom("G"),
                Type = GuardianType.OfEmail,
                VerifierId = HashHelper.ComputeFrom("V")
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
            MethodName = "GuardianAdded",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };

        //step3: handle event and write result to blockStateSet
        var guardianAddedProcessor = GetRequiredService<GuardianAddedLogEventProcessor>();
        await guardianAddedProcessor.HandleEventAsync(logEventInfo, logEventContext);

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await Task.Delay(2000);
    }
}