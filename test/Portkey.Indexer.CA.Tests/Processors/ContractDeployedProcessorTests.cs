using AElf.CSharp.Core.Extension;
using AElf.Standards.ACS0;
using AElf.Types;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Nethereum.Hex.HexConvertors.Extensions;
using Portkey.Indexer.CA.Entities;
using Portkey.Indexer.CA.Processors;
using Portkey.Indexer.CA.Tests.Helper;
using Shouldly;
using Xunit;

namespace Portkey.Indexer.CA.Tests.Processors;

public class ContractDeployedProcessorTests:PortkeyIndexerCATestBase
{
    private readonly IAElfIndexerClientEntityRepository<TokenInfoIndex, TransactionInfo> _tokenInfoIndexRepository;
    
    public ContractDeployedProcessorTests()
    {
        _tokenInfoIndexRepository = GetRequiredService<IAElfIndexerClientEntityRepository<TokenInfoIndex, TransactionInfo>>();
    }

    [Fact]
    public async Task HandleEventAsync_Test()
    {
        const string chainId = "AELF";
        const string blockHash = "0f4f79c709ee39c597795689f99be3c1384148dbb1a0b1b0fa21fc91229164e3";
        const string previousBlockHash = "f2316fb0e7646259a4238d8cd4700c9c6451a432e89df48c2368418c55c22b81";
        const string transactionId = "7a4c16a8aa4bb415b1128d060bb3e356ca7bab9ff77be5838a0ce5c4f5b1fe19";
        const long blockHeight = 120;
        
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
        var contractDeployed = new ContractDeployed
        {
            Address = Address.FromPublicKey("BBB".HexToByteArray())
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(contractDeployed.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId= chainId;
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
        
        //step3: handle event and write result to blockStateSet
        var contractDeployedProcessor = GetRequiredService<ContractDeployedProcessor>();
        await contractDeployedProcessor.HandleEventAsync(logEventInfo, logEventContext);
        contractDeployedProcessor.GetContractAddress(chainId);
        
        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await Task.Delay(2000);
        
        //step5: check result
        var tokenInfoIndexData = await _tokenInfoIndexRepository.GetAsync(IdGenerateHelper.GetId(chainId, "SYB"));
        tokenInfoIndexData.ShouldNotBeNull();
        tokenInfoIndexData.Symbol.ShouldBe("SYB");
        tokenInfoIndexData.TokenName.ShouldBe("SYB Token");
    }
}