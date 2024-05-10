using AElf;
using AElf.Contracts.MultiToken;
using AElf.CSharp.Core.Extension;
using AElf.Types;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Nethereum.Hex.HexConvertors.Extensions;
using NSubstitute.Core;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Portkey.Indexer.CA.GraphQL;
using Portkey.Indexer.CA.Processors;
using Portkey.Indexer.CA.Tests.Helper;
using Shouldly;
using Volo.Abp.ObjectMapping;
using Xunit;
using Query = Portkey.Indexer.CA.GraphQL.Query;

namespace Portkey.Indexer.CA.Tests.Processors;

public class TokenApprovedProcessorTests : PortkeyIndexerCATestBase
{
    private readonly IAElfIndexerClientEntityRepository<CAHolderTokenApprovedIndex, TransactionInfo> _caHolderTokenApprovedIndexRepository;

    private readonly IObjectMapper _objectMapper;
    public TokenApprovedProcessorTests()
    {
        _caHolderTokenApprovedIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderTokenApprovedIndex, TransactionInfo>>();
        _objectMapper = GetRequiredService<IObjectMapper>();
    }

    [Fact]
    public async Task TokenApprovedTest()
    {
        await CreateHolder();

        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;
        var tokenApprovedProcessor = GetRequiredService<TokenApprovedProcessor>();
        var tokenUnApprovedProcessor = GetRequiredService<TokenUnApprovedProcessor>();

        //step1: create blockStateSet
        var blockStateSet = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash
        };

        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        //step2: create logEventInfo
        var approved = new Approved
        {
            Amount = 1000000000,
            Symbol = "ELF",
            Owner = Address.FromPublicKey("AAA".HexToByteArray()),
            Spender = Address.FromPublicKey("DDD".HexToByteArray())
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(approved.ToLogEvent());
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
            To = "TokenAddress",
            MethodName = "Approve",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };

        //step3: handle event and write result to blockStateSet
        await tokenApprovedProcessor.HandleEventAsync(logEventInfo, logEventContext);
        tokenApprovedProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await Task.Delay(2000);

        // CAHolderTokenApproved
        var caHolderTokenApprovedIndex = await _caHolderTokenApprovedIndexRepository.GetAsync(IdGenerateHelper.GetId(
            chainId, Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(), Address.FromPublicKey("DDD".HexToByteArray()).ToBase58()));
        caHolderTokenApprovedIndex.Spender.ShouldBe(Address.FromPublicKey("DDD".HexToByteArray()).ToBase58());
        caHolderTokenApprovedIndex.CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        caHolderTokenApprovedIndex.BatchApprovedAmount.ShouldBe(0);

        approved.Symbol = "*";
        logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(approved.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId = chainId;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        //step3: handle event and write result to blockStateSet
        await tokenApprovedProcessor.HandleEventAsync(logEventInfo, logEventContext);
        tokenApprovedProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await Task.Delay(2000);
        
        caHolderTokenApprovedIndex = await _caHolderTokenApprovedIndexRepository.GetAsync(IdGenerateHelper.GetId(
            chainId, Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(), Address.FromPublicKey("DDD".HexToByteArray()).ToBase58()));
        caHolderTokenApprovedIndex.Spender.ShouldBe(Address.FromPublicKey("DDD".HexToByteArray()).ToBase58());
        caHolderTokenApprovedIndex.CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        caHolderTokenApprovedIndex.BatchApprovedAmount.ShouldBe(1000000000);
        
        // unapprove test
        var unapproved = new UnApproved
        {
            Amount = 1000000000,
            Symbol = "*",
            Owner = Address.FromPublicKey("AAA".HexToByteArray()),
            Spender = Address.FromPublicKey("DDD".HexToByteArray())
        };
        var logEventInfo1 = LogEventHelper.ConvertAElfLogEventToLogEventInfo(unapproved.ToLogEvent());
        logEventInfo1.BlockHeight = blockHeight;
        logEventInfo1.ChainId = chainId;
        logEventInfo1.BlockHash = blockHash;
        logEventInfo1.TransactionId = transactionId;
        //step3: handle event and write result to blockStateSet
        await tokenUnApprovedProcessor.HandleEventAsync(logEventInfo1, logEventContext);
        tokenUnApprovedProcessor.GetContractAddress("AELF");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await Task.Delay(2000);
        
        caHolderTokenApprovedIndex = await _caHolderTokenApprovedIndexRepository.GetAsync(IdGenerateHelper.GetId(
            chainId, Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(), Address.FromPublicKey("DDD".HexToByteArray()).ToBase58()));
        caHolderTokenApprovedIndex.Spender.ShouldBe(Address.FromPublicKey("DDD".HexToByteArray()).ToBase58());
        caHolderTokenApprovedIndex.CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        caHolderTokenApprovedIndex.BatchApprovedAmount.ShouldBe(0);

        var pageResultDto = await Query.CAHolderTokenApprovedAsync(_caHolderTokenApprovedIndexRepository, _objectMapper, new GetCAHolderTokenApprovedDto()
        {
            ChainId = "AELF",
            CAAddresses = new List<string>()
            {
                Address.FromPublicKey("AAA".HexToByteArray()).ToBase58()
            }
        });
        pageResultDto.Data.Count.ShouldBe(1);
        pageResultDto.TotalRecordCount.ShouldBe(1);
        pageResultDto.Data[0].Spender.ShouldBe(Address.FromPublicKey("DDD".HexToByteArray()).ToBase58());
        pageResultDto.Data[0].CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        pageResultDto.Data[0].ChainId.ShouldBe("AELF");
        pageResultDto.Data[0].BatchApprovedAmount.ShouldBe(0);
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