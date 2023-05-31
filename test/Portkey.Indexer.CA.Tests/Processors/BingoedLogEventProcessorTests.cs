using AElf;
using AElf.CSharp.Core.Extension;
using AElf.Types;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Nethereum.Hex.HexConvertors.Extensions;
using Portkey.Contracts.BingoGameContract;
using Portkey.Indexer.CA.Entities;
using Portkey.Indexer.CA.Processors;
using Portkey.Indexer.CA.Tests.Helper;
using Shouldly;
using Xunit;

namespace Portkey.Indexer.CA.Tests.Processors;

public class BingoedProcessorTests: PortkeyIndexerCATestBase
{
    private readonly IAElfIndexerClientEntityRepository<BingoGameIndex, LogEventInfo> _bingoGameIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, LogEventInfo>
        _caHolderTransactionRepository;
    private readonly IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> _repository;
    
    public BingoedProcessorTests()
    {
        _bingoGameIndexRepository = GetRequiredService<IAElfIndexerClientEntityRepository<BingoGameIndex, LogEventInfo>>();
        _caHolderTransactionRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, LogEventInfo>>();
        _repository = GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo>>();
    }
    [Fact]
    public async Task HandleBingoedLogEventAsync_Test(){
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
            Amount = 100000000,
            Award = 100000000,
            IsComplete = true,
            Type = BingoType.Large,
            Dices = new DiceList
            {
                Dices = {1, 2, 3, 4, 5, 6}
            },
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
            MethodName = "AddGuardian",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"30000000\"}" },
                { "ResourceFee", "{\"ELF\":\"30000000\"}" }
            },
            BlockTime = DateTime.UtcNow
        };
        var bingoedLogEventProcessor = GetRequiredService<BingoedProcessor>();
        
        await bingoedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        
        bingoedLogEventProcessor.GetContractAddress(chainId);
        
        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        var bingoGameIndexData = await _bingoGameIndexRepository.GetAsync(IdGenerateHelper.GetId(chainId, bingoed.PlayerAddress.ToBase58()));
        bingoGameIndexData.BingoType.ShouldBe((int)bingoed.Type);
        bingoGameIndexData.BingoBlockHeight.ShouldBe(bingoed.BingoBlockHeight);
        bingoGameIndexData.PlayBlockHeight.ShouldBe(bingoed.PlayBlockHeight);
        bingoGameIndexData.PlayerAddress.ShouldBe(bingoed.PlayerAddress.ToBase58());
        bingoGameIndexData.Amount.ShouldBe(bingoed.Amount);
        bingoGameIndexData.Award.ShouldBe(bingoed.Award);
        bingoGameIndexData.IsComplete.ShouldBe(bingoed.IsComplete);
        bingoGameIndexData.Dices.ShouldBe(bingoed.Dices.Dices.Select(d => (int)d));
        bingoGameIndexData.PlayId.ShouldBe(bingoed.PlayId.ToHex());
        
    }
}