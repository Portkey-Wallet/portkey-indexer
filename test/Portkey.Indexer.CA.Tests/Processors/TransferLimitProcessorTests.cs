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
public class TransferLimitProcessorTests : PortkeyIndexerCATestBase
{
    private readonly IAElfIndexerClientEntityRepository<TransferLimitIndex, LogEventInfo>
        _transferLimitIndexRepository;

    private readonly IObjectMapper _objectMapper;

    public TransferLimitProcessorTests()
    {
        _objectMapper = GetRequiredService<IObjectMapper>();
        _transferLimitIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<TransferLimitIndex, LogEventInfo>>();
    }


    [Fact]
    public async Task TransferLimitChangedAsync_Test()
    {
        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;
        const string defaultSymbol = "ELF";
        const long defaultTransferLimit = 10000000;
        var CaHash = HashHelper.ComputeFrom("test@google.com");


        var tokenCreatedProcessor = GetRequiredService<TransferLimitChangedLogEventProcessor>();
        tokenCreatedProcessor.GetContractAddress(chainId);
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);
        var transferLimitChanged = new TransferLimitChanged()
        {
            CaHash = CaHash,
            Symbol = defaultSymbol,
            SingleLimit = defaultTransferLimit,
            DailyLimit = defaultTransferLimit
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(transferLimitChanged.ToLogEvent());
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
            TransactionId = transactionId
        };

        await tokenCreatedProcessor.HandleEventAsync(logEventInfo, logEventContext);

        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await Task.Delay(2000);
        var tokenInfoIndexData =
            await _transferLimitIndexRepository.GetAsync(IdGenerateHelper.GetId(chainId, transactionId));
        tokenInfoIndexData.BlockHeight.ShouldBe(blockHeight);
        tokenInfoIndexData.Symbol.ShouldBe(defaultSymbol);
        tokenInfoIndexData.SingleLimit.ShouldBe(defaultTransferLimit);
        tokenInfoIndexData.DailyLimit.ShouldBe(defaultTransferLimit);
    }

    [Fact]
    public async Task QueryCAHolderTransferLimitTests()
    {
        await TransferLimitChangedAsync_Test();

        var result = await Query.CAHolderTransferLimit(_transferLimitIndexRepository, _objectMapper,
            new GetCAHolderTransferLimitDto()
            {
                CAHash = HashHelper.ComputeFrom("test@google.com").ToHex(),
            });
        result.TotalRecordCount.ShouldBe(1);
        result.Data.Count.ShouldBe(1);
    }
}