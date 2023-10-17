using AElf.Types;
using AElfIndexer.Client;
using AElfIndexer.Grains.State.Client;
using Force.DeepCloner;
using Nethereum.Hex.HexConvertors.Extensions;
using Portkey.Indexer.CA.Entities;
using Portkey.Indexer.CA.GraphQL;
using Portkey.Indexer.CA.Handlers;
using Shouldly;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace Portkey.Indexer.CA.Tests.Processors;

public class CAHolderTransactionHandlerTests : PortkeyIndexerCATestBase
{
    private readonly IAElfIndexerClientEntityRepository<TransactionInfoIndex, TransactionInfo>
        _transactionInfoIndexRepository;
    private readonly IObjectMapper _objectMapper;

    public CAHolderTransactionHandlerTests()
    {
        _transactionInfoIndexRepository = GetRequiredService<IAElfIndexerClientEntityRepository<TransactionInfoIndex, TransactionInfo>>();
        _objectMapper = GetRequiredService<IObjectMapper>();
    }


    [Fact]
    public async Task ProcessTransactionsTests()
    {
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
        var transactionInfo = new TransactionInfo
        {
            ChainId = chainId,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId,
            Params = "{ \"to\": \"ca\", \"symbol\": \"ELF\", \"amount\": \"100000000000\" }",
            To = caContractAddress,
            MethodName = "ValidateCAHolderInfoWithManagerInfosExists",
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

        var transactionInfoIndex = await _transactionInfoIndexRepository.GetAsync(IdGenerateHelper.GetId(
            transactionInfo.ChainId, transactionInfo.TransactionId));
        transactionInfoIndex.TransactionId.ShouldBe(transactionId);
        transactionInfoIndex.Confirmed.ShouldBeTrue();
        transactionInfoIndex.ChainId.ShouldBe(chainId);
        transactionInfoIndex.MethodName.ShouldBe("ValidateCAHolderInfoWithManagerInfosExists");

        var transaction1 = transactionInfo.DeepClone();
        transaction1.TransactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2d11";
        transaction1.MethodName = "SyncHolderInfo";
        transaction1.Confirmed = false;
        await caHolderTransactionHandler.ProcessTransactionListAsync(new List<TransactionInfo>{transactionInfo, transaction1});
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await Task.Delay(2000);
        
        transactionInfoIndex = await _transactionInfoIndexRepository.GetAsync(IdGenerateHelper.GetId(
            transaction1.ChainId, transaction1.TransactionId));
        transactionInfoIndex.TransactionId.ShouldBe("c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2d11");
        transactionInfoIndex.Confirmed.ShouldBeFalse();
        transactionInfoIndex.ChainId.ShouldBe(chainId);
        transactionInfoIndex.MethodName.ShouldBe("SyncHolderInfo");
    }

    [Fact]
    public async Task QueryTransactionInfosTests()
    {
        await ProcessTransactionsTests();
        var result = await Query.QueryTransactionInfos(_transactionInfoIndexRepository, _objectMapper, new GetTransactionIdsDto
        {
            ChainId = "AELF",
            TransactionList = new List<string>
            {
                "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2",
                "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2d11"
            }
        });
        result.Count.ShouldBe(2);
        result.FirstOrDefault(
                t => t.TransactionId == "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2")
            .ShouldNotBeNull();
        result.FirstOrDefault(
                t => t.TransactionId == "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2d11")
            .ShouldNotBeNull();
        
        result = await Query.QueryTransactionInfos(_transactionInfoIndexRepository, _objectMapper, new GetTransactionIdsDto
        {
            ChainId = "AELF",
            TransactionList = new List<string>
            {
                "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2",
                "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2d11"
            },
            Confirmed = true
        });
        result.Count.ShouldBe(1);
        result[0].TransactionId.ShouldBe("c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2");
        
        result = await Query.QueryTransactionInfos(_transactionInfoIndexRepository, _objectMapper, new GetTransactionIdsDto
        {
            ChainId = "AELF",
            TransactionList = new List<string>
            {
                "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2",
                "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2d11"
            },
            Confirmed = false
        });
        result.Count.ShouldBe(1);
        result[0].TransactionId.ShouldBe("c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2d11");
        
        result = await Query.QueryTransactionInfos(_transactionInfoIndexRepository, _objectMapper, new GetTransactionIdsDto
        {
            ChainId = "tDVV",
            TransactionList = new List<string>
            {
                "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2",
                "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2d11"
            }
        });
        result.Count.ShouldBe(0);
    }

    [Fact]
    public async Task ProcessTransactionsFailTests()
    {
        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da3";
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
        var transactionInfo = new TransactionInfo
        {
            ChainId = chainId,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId,
            Params = "{ \"to\": \"ca\", \"symbol\": \"ELF\", \"amount\": \"100000000000\" }",
            To = caContractAddress,
            MethodName = "CreateHolderInfo",
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
        
        var transactionInfoIndex = await _transactionInfoIndexRepository.GetAsync(IdGenerateHelper.GetId(
            transactionInfo.ChainId, transactionInfo.TransactionId));
        transactionInfoIndex.ShouldBeNull();
        
        transactionInfo.MethodName = "ValidateCAHolderInfoWithManagerInfosExists";
        transactionInfo.To = "test";
        await caHolderTransactionHandler.ProcessTransactionListAsync(new List<TransactionInfo>{transactionInfo});
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await Task.Delay(2000);
        
        transactionInfoIndex = await _transactionInfoIndexRepository.GetAsync(IdGenerateHelper.GetId(
            transactionInfo.ChainId, transactionInfo.TransactionId));
        transactionInfoIndex.ShouldBeNull();
    }
}