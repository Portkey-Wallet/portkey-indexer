using AElf;
using AElf.CSharp.Core.Extension;
using AElf.Types;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Nethereum.Hex.HexConvertors.Extensions;
using Portkey.Contracts.BeangoTownContract;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Portkey.Indexer.CA.Handlers;
using Portkey.Indexer.CA.Processors;
using Portkey.Indexer.CA.Tests.Helper;
using Shouldly;
using Xunit;

namespace Portkey.Indexer.CA.Tests.Processors;

public class ForwardCallTransactionHandlerTests : PortkeyIndexerCATestBase
{
    private readonly IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>
        _caTransactionIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo>
        _caHolderIndexRepository;

    public ForwardCallTransactionHandlerTests()
    {
        _caTransactionIndexRepository = GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>>();
        _caHolderIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo>>();
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
            Creator = Address.FromPublicKey("BBB".HexToByteArray()),
            Manager = Address.FromPublicKey("CCC".HexToByteArray()),
            ExtraData = "ExtraData"
        };
        caHolderCreated.CaAddress =
            ConvertVirtualAddressToContractAddress(caHolderCreated.CaHash,
                Address.FromPublicKey("AAA".HexToByteArray()));
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
    public async Task ProcessTransactionsTests()
    {
        await CreateHolder();
        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;
        var caContractAddress = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58();
        var caHolderTransactionHandler = GetRequiredService<CAHolderTransactionHandler>();
        
        var blockStateSet = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);
        var managerForwardCallInput = new ManagerForwardCallInput
        {
            CaHash = HashHelper.ComputeFrom("test@google.com"),
            MethodName = "BingoTown",
            ContractAddress = Address.FromPublicKey("AAA".HexToByteArray()),
            Args = ByteString.CopyFrom(new Played().ToByteArray())
        };
        var transactionInfo = new TransactionInfo
        {
            ChainId = chainId,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId,
            Params = ByteString.CopyFrom(managerForwardCallInput.ToByteArray()).ToBase64(),
            To = caContractAddress,
            MethodName = "ManagerForwardCall",
            ExtraProperties = new Dictionary<string, string>
            {
                {"TransactionFee", "{\"ELF\":\"30000000\"}"},
                {"ResourceFee", "{\"ELF\":\"30000000\"}"}
            },
            BlockTime = DateTime.UtcNow,
            Confirmed = true
        };

        await caHolderTransactionHandler.ProcessTransactionListAsync(new List<TransactionInfo>{transactionInfo});
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await Task.Delay(2000);

        var caAddress =
            ConvertVirtualAddressToContractAddress(managerForwardCallInput.CaHash,
                managerForwardCallInput.ContractAddress).ToBase58();
        
        var caHolder = await _caHolderIndexRepository.GetAsync(IdGenerateHelper.GetId(
            transactionInfo.ChainId, caAddress));
        caHolder.CAHash.ShouldBe(HashHelper.ComputeFrom("test@google.com").ToHex());
        var caTransactionIndex = await _caTransactionIndexRepository.GetAsync(IdGenerateHelper.GetId(
            transactionInfo.BlockHash, transactionInfo.TransactionId));
        caTransactionIndex.FromAddress.ShouldBe(caAddress);
        caTransactionIndex.Timestamp.ShouldBe(transactionInfo.BlockTime.ToTimestamp().Seconds);

        managerForwardCallInput.MethodName = "Play";
        transactionInfo.Params = ByteString.CopyFrom(managerForwardCallInput.ToByteArray()).ToBase64();
        transactionInfo.TransactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da3";
        await caHolderTransactionHandler.ProcessTransactionListAsync(new List<TransactionInfo>{transactionInfo});
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await Task.Delay(2000);
        
        caTransactionIndex = await _caTransactionIndexRepository.GetAsync(IdGenerateHelper.GetId(
            transactionInfo.BlockHash, transactionInfo.TransactionId));
        caTransactionIndex.ShouldBeNull();
    }
    
    private Address ConvertVirtualAddressToContractAddress(
        Hash virtualAddress,
        Address contractAddress)
    {
        return Address.FromPublicKey(contractAddress.Value.Concat<byte>((IEnumerable<byte>) virtualAddress.Value.ToByteArray().ComputeHash()).ToArray<byte>());
    }
}