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
    private readonly IAElfIndexerClientEntityRepository<TransferLimitIndex, TransactionInfo>
        _transferLimitIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<TransferSecurityThresholdIndex, TransactionInfo>
        _transferSecurityThresholdIndexRepository;

    private readonly IObjectMapper _objectMapper;

    public TransferLimitProcessorTests()
    {
        _objectMapper = GetRequiredService<IObjectMapper>();
        _transferLimitIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<TransferLimitIndex, TransactionInfo>>();
        _transferSecurityThresholdIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<TransferSecurityThresholdIndex, TransactionInfo>>();
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
        var caHash = HashHelper.ComputeFrom("test@google.com");

        var tokenCreatedProcessor = GetRequiredService<TransferLimitChangedProcessor>();
        tokenCreatedProcessor.GetContractAddress(chainId);
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

        var transferLimitChanged = new TransferLimitChanged()
        {
            CaHash = caHash,
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

        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);

        await Task.Delay(2000);
        var tokenInfoIndexData =
            await _transferLimitIndexRepository.GetAsync(IdGenerateHelper.GetId(chainId, caHash.ToHex(),
                nameof(TransferLimitChanged), defaultSymbol));
        tokenInfoIndexData.BlockHeight.ShouldBe(blockHeight);
        tokenInfoIndexData.Symbol.ShouldBe(defaultSymbol);
        tokenInfoIndexData.SingleLimit.ShouldBe(defaultTransferLimit);
        tokenInfoIndexData.DailyLimit.ShouldBe(defaultTransferLimit);
    }

    [Fact]
    public async Task TransferSecurityThresholdChangedAsync_Test()
    {
        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;
        const string defaultSymbol = "ELF";
        const long defaultGuardianThreshold = 1;
        const long defaultBalanceThreshold = 1000;

        var transferSecurityThresholdChangedProcessor =
            GetRequiredService<TransferSecurityThresholdChangedLogEventProcessor>();
        transferSecurityThresholdChangedProcessor.GetContractAddress(chainId);
        var blockStateSet = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);
        var transferLimitChanged = new TransferSecurityThresholdChanged
        {
            Symbol = defaultSymbol,
            GuardianThreshold = defaultGuardianThreshold,
            BalanceThreshold = defaultBalanceThreshold
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

        await transferSecurityThresholdChangedProcessor.HandleEventAsync(logEventInfo, logEventContext);

        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await Task.Delay(2000);

        var tokenInfoIndexData =
            await _transferSecurityThresholdIndexRepository.GetAsync(IdGenerateHelper.GetId(chainId, defaultSymbol,
                nameof(TransferSecurityThresholdChanged)));
        tokenInfoIndexData.BlockHeight.ShouldBe(blockHeight);
        tokenInfoIndexData.Symbol.ShouldBe(defaultSymbol);
        tokenInfoIndexData.BalanceThreshold.ShouldBe(defaultBalanceThreshold);
        tokenInfoIndexData.GuardianThreshold.ShouldBe(defaultGuardianThreshold);
    }

    [Fact]
    public async Task QueryCAHolderTransferLimitTests()
    {
        await TransferLimitChangedAsync_Test();

        var result = await Query.CAHolderTransferLimitAsync(_transferLimitIndexRepository, _objectMapper,
            new GetCAHolderTransferLimitDto
            {
                CAHash = HashHelper.ComputeFrom("test@google.com").ToHex(),
            });
        result.TotalRecordCount.ShouldBe(1);
        result.Data.Count.ShouldBe(1);
    }

    [Fact]
    public async Task QueryTransferSecurityThresholdListTests()
    {
        await TransferSecurityThresholdChangedAsync_Test();

        var result = await Query.TransferSecurityThresholdListAsync(_transferSecurityThresholdIndexRepository, _objectMapper,
            new GetTransferSecurityThresholdChangedDto());
        result.TotalRecordCount.ShouldBe(1);
        result.Data.Count.ShouldBe(1);
    }
}