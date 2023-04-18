using AElf;
using AElf.CSharp.Core.Extension;
using AElf.Types;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Client.Providers;
using AElfIndexer.Grains;
using AElfIndexer.Grains.State.Client;
using Nethereum.Hex.HexConvertors.Extensions;
using Orleans;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Portkey.Indexer.CA.GraphQL;
using Portkey.Indexer.CA.Tests.Helper;
using Portkey.Indexer.CA.Processors;
using Portkey.Indexer.Orleans.TestBase;
using Shouldly;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace Portkey.Indexer.CA.Tests.Processors;

[Collection(ClusterCollection.Name)]
public sealed class CAHolderCreatedProcessorTests:PortkeyIndexerCATestBase
{
    private readonly IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> _caHolderIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<CAHolderManagerIndex, LogEventInfo> _caHolderManagerIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<LoginGuardianIndex, LogEventInfo> _loginGuardianRepository;
    private readonly IObjectMapper _objectMapper;

    public CAHolderCreatedProcessorTests()
    {
        _caHolderIndexRepository = GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo>>();
        _caHolderManagerIndexRepository = GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderManagerIndex, LogEventInfo>>();
        _loginGuardianRepository = GetRequiredService<IAElfIndexerClientEntityRepository<LoginGuardianIndex, LogEventInfo>>();
        _objectMapper = GetRequiredService<IObjectMapper>();
    }

    [Fact]
    public async Task HandleEventAsync_Test()
    {
        //step1: create blockStateSet
        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;
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
        var caHolderCreatedProcessor = GetRequiredService<CAHolderCreatedProcessor>();
        await caHolderCreatedProcessor.HandleEventAsync(logEventInfo, logEventContext);
        caHolderCreatedProcessor.GetContractAddress(chainId);
        
        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await Task.Delay(2000);
        
        //step5: check result
        var caHolderIndexData = await _caHolderIndexRepository.GetAsync(chainId+"-"+caHolderCreated.CaAddress.ToBase58());
        caHolderIndexData.BlockHeight.ShouldBe(blockHeight);
        caHolderIndexData.ManagerInfos.FirstOrDefault().Address.ShouldBe(caHolderCreated.Manager.ToBase58());
        var caHolderManagerIndexData = await _caHolderManagerIndexRepository.GetAsync(chainId+"-"+caHolderCreated.Manager.ToBase58());
        caHolderManagerIndexData.BlockHeight.ShouldBe(blockHeight);
        caHolderManagerIndexData.CAAddresses.FirstOrDefault().ShouldBe(caHolderCreated.CaAddress.ToBase58());
    }
    
    [Fact]
    public async Task Query_CAHolderInfo_Test()
    {
        await HandleEventAsync_Test();
        await Task.Delay(1000);
        var param=new GetCAHolderInfoDto()
        {
            CAHash = HashHelper.ComputeFrom("test@google.com").ToHex()
        };
        var result = await Query.CAHolderInfo(_caHolderIndexRepository,_loginGuardianRepository, _objectMapper, param);
        result.Count.ShouldBe(1);
        result.FirstOrDefault().CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
    }
}