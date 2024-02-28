using AElf;
using AElf.Contracts.MultiToken;
using AElf.CSharp.Core.Extension;
using AElf.Types;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Client.Providers;
using AElfIndexer.Grains.State.Client;
using Nethereum.Hex.HexConvertors.Extensions;
using Orleans;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Portkey.Indexer.CA.GraphQL;
using Portkey.Indexer.CA.Processors;
using Portkey.Indexer.CA.Tests.Helper;
using Shouldly;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace Portkey.Indexer.CA.Tests.Processors;

public class TokenLogEventProcessorTests : PortkeyIndexerCATestBase
{
    private readonly IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> _tokenInfoIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<NFTCollectionInfoIndex, LogEventInfo>
        _nftCollectionInfoIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<NFTInfoIndex, LogEventInfo> _nftInfoIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<CAHolderTokenBalanceIndex, LogEventInfo>
        _caHolderTokenBalanceIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<CAHolderSearchTokenNFTIndex, LogEventInfo>
        _caHolderSearchTokenNFTIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<CAHolderNFTCollectionBalanceIndex, LogEventInfo>
        _caHolderNFTCollectionBalanceRepository;

    private readonly IAElfIndexerClientEntityRepository<CAHolderNFTBalanceIndex, LogEventInfo>
        _caHolderNFTBalanceIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>
        _caHolderTransactionIndexRepository;

    private readonly IAElfIndexerClientEntityRepository<TransactionFeeChangedIndex, LogEventInfo>
        transactionFeeRepository;

    private readonly IAElfIndexerClientEntityRepository<CompatibleCrossChainTransferIndex, TransactionInfo>
        _compatibleCrossChainTransferRepository;

    private readonly IObjectMapper _objectMapper;

    const string holderBEmail = "testB@google.com";
    const string caaddressB = "AAAA";
    const string creatorB = "BBBB";
    const string managerB = "CCCC";
    const string managerC = "DDDD";
    const string chainId = "AELF";
    const string chainIdSide = "tDVV";
    const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
    const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
    const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
    const long blockHeight = 100;
    const string crossChainTransferMethodName = "CrossChainTransfer";
    const string crossChainReceivedMethodName = "CrossChainReceiveToken";
    const string transferMethodName = "Transferred";
    const string contractAddress = "CAAddress";
    private const string defaultManager = "CCC";
    private const string Params = "{ \"to\": \"ca\", \"symbol\": \"ELF\", \"amount\": \"100000000000\" }";

    private static Dictionary<string, string> extraProperties = new Dictionary<string, string>
    {
        { "TransactionFee", "{\"ELF\":\"30000000\"}" },
        { "ResourceFee", "{\"ELF\":\"30000000\"}" }
    };

    const string defaultSymbol = "READ";


    public TokenLogEventProcessorTests()
    {
        _caHolderTokenBalanceIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderTokenBalanceIndex, LogEventInfo>>();
        _caHolderNFTCollectionBalanceRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderNFTCollectionBalanceIndex, LogEventInfo>>();
        _caHolderNFTBalanceIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderNFTBalanceIndex, LogEventInfo>>();
        _tokenInfoIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo>>();
        _nftCollectionInfoIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<NFTCollectionInfoIndex, LogEventInfo>>();
        _nftCollectionInfoIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<NFTCollectionInfoIndex, LogEventInfo>>();
        _nftInfoIndexRepository = GetRequiredService<IAElfIndexerClientEntityRepository<NFTInfoIndex, LogEventInfo>>();
        _caHolderSearchTokenNFTIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderSearchTokenNFTIndex, LogEventInfo>>();
        _caHolderTransactionIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>>();
        transactionFeeRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<TransactionFeeChangedIndex, LogEventInfo>>();
        _compatibleCrossChainTransferRepository = 
            GetRequiredService<IAElfIndexerClientEntityRepository<CompatibleCrossChainTransferIndex, TransactionInfo>>();
        _objectMapper = GetRequiredService<IObjectMapper>();
    }


    public async Task<(CAHolderCreated, string)> CreateHolder(string email = "test@google.com",
        string caaddress = "AAA", string creator = "BBB", string manager = defaultManager, string chainId = "AELF")
    {
        var caHolderCreatedProcessor = GetRequiredService<CAHolderCreatedLogEventProcessor>();
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        //step1: create blockStateSet
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);
        var caHolderCreated = new CAHolderCreated
        {
            CaHash = HashHelper.ComputeFrom(email),
            CaAddress = Address.FromPublicKey(caaddress.HexToByteArray()),
            Creator = Address.FromPublicKey(creator.HexToByteArray()),
            Manager = Address.FromPublicKey(manager.HexToByteArray()),
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
            TransactionId = transactionId
        };

        //step3: handle event and write result to blockStateSet
        await caHolderCreatedProcessor.HandleEventAsync(logEventInfo, logEventContext);

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await Task.Delay(2000);

        return (caHolderCreated, blockStateSetKey);
    }


    [Fact]
    public async Task HandleTokenCreatedAsync_Test()
    {
        const string symbol = defaultSymbol;
        const string tokenName = "READ Token";
        const long totalSupply = 0;
        const int decimals = 8;
        const bool isBurnable = true;
        const int issueChainId = 9992731;

        var tokenCreatedProcessor = GetRequiredService<TokenCreatedProcessor>();
        tokenCreatedProcessor.GetContractAddress(chainId);
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        //step1: create blockStateSet
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        //step2: create logEventInfo
        var tokenCreated = new TokenCreated()
        {
            Symbol = symbol,
            TokenName = tokenName,
            TotalSupply = totalSupply,
            Decimals = decimals,
            Issuer = Address.FromPublicKey("AAA".HexToByteArray()),
            IsBurnable = isBurnable,
            IssueChainId = issueChainId,
            ExternalInfo = new ExternalInfo()
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(tokenCreated.ToLogEvent());
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

        var tokenInfoIndexData = await _tokenInfoIndexRepository.GetAsync(chainId + "-" + symbol);
        tokenInfoIndexData.BlockHeight.ShouldBe(blockHeight);
        tokenInfoIndexData.Decimals.ShouldBe(decimals);
        tokenInfoIndexData.Symbol.ShouldBe(symbol);
        tokenInfoIndexData.TokenName.ShouldBe(tokenName);
        tokenInfoIndexData.TotalSupply.ShouldBe(totalSupply);
        tokenInfoIndexData.Decimals.ShouldBe(decimals);
        tokenInfoIndexData.IsBurnable.ShouldBe(isBurnable);
        tokenInfoIndexData.IssueChainId.ShouldBe(issueChainId);
    }

    [Fact]
    public async Task HandleNFTCollectionCreatedAsync_Test()
    {
        const string symbol = "READ-0";
        const string tokenName = "READ Token";
        const long totalSupply = 0;
        const int decimals = 8;
        const bool isBurnable = true;
        const int issueChainId = 9992731;

        var tokenCreatedProcessor = GetRequiredService<TokenCreatedProcessor>();
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        var tokenCreated = new TokenCreated()
        {
            Symbol = symbol,
            TokenName = tokenName,
            TotalSupply = totalSupply,
            Decimals = decimals,
            Issuer = Address.FromPublicKey("AAA".HexToByteArray()),
            IsBurnable = isBurnable,
            IssueChainId = issueChainId,
            ExternalInfo = new ExternalInfo()
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(tokenCreated.ToLogEvent());
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

        var tokenInfoIndexData = await _nftCollectionInfoIndexRepository.GetAsync(chainId + "-" + symbol);
        tokenInfoIndexData.BlockHeight.ShouldBe(blockHeight);
        tokenInfoIndexData.Decimals.ShouldBe(decimals);
        tokenInfoIndexData.Symbol.ShouldBe(symbol);
        tokenInfoIndexData.TokenName.ShouldBe(tokenName);
        tokenInfoIndexData.TotalSupply.ShouldBe(totalSupply);
        tokenInfoIndexData.Decimals.ShouldBe(decimals);
        tokenInfoIndexData.IsBurnable.ShouldBe(isBurnable);
        tokenInfoIndexData.IssueChainId.ShouldBe(issueChainId);
    }

    [Fact]
    public async Task HandleNFTCollectionCreated_Default_Async_Test()
    {
        const string symbol = "READ-0";
        const string tokenName = "READ Token";
        const long totalSupply = 0;
        const int decimals = 8;
        const bool isBurnable = true;
        const int issueChainId = 9992731;

        var tokenCreatedProcessor = GetRequiredService<TokenCreatedProcessor>();
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        var tokenCreated = new TokenCreated()
        {
            Symbol = symbol,
            TokenName = tokenName,
            TotalSupply = totalSupply,
            Decimals = decimals,
            Issuer = Address.FromPublicKey("AAA".HexToByteArray()),
            IsBurnable = isBurnable,
            IssueChainId = issueChainId
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(tokenCreated.ToLogEvent());
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

        var tokenInfoIndexData = await _nftCollectionInfoIndexRepository.GetAsync(chainId + "-" + symbol);
        tokenInfoIndexData.BlockHeight.ShouldBe(blockHeight);
        tokenInfoIndexData.Decimals.ShouldBe(decimals);
        tokenInfoIndexData.Symbol.ShouldBe(symbol);
        tokenInfoIndexData.TokenName.ShouldBe(tokenName);
        tokenInfoIndexData.TotalSupply.ShouldBe(totalSupply);
        tokenInfoIndexData.Decimals.ShouldBe(decimals);
        tokenInfoIndexData.IsBurnable.ShouldBe(isBurnable);
        tokenInfoIndexData.IssueChainId.ShouldBe(issueChainId);
    }

    [Fact]
    public async Task HandleNFTCollectionCreated_Null_Async_Test()
    {
        const string symbol = "READ-0";
        const string tokenName = "READ Token";
        const long totalSupply = 0;
        const int decimals = 8;
        const bool isBurnable = true;
        const int issueChainId = 9992731;

        var tokenCreatedProcessor = GetRequiredService<TokenCreatedProcessor>();
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        var tokenCreated = new TokenCreated()
        {
            Symbol = symbol,
            TokenName = tokenName,
            TotalSupply = totalSupply,
            Decimals = decimals,
            Issuer = Address.FromPublicKey("AAA".HexToByteArray()),
            IsBurnable = isBurnable,
            IssueChainId = issueChainId,
            ExternalInfo = null
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(tokenCreated.ToLogEvent());
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

        var tokenInfoIndexData = await _nftCollectionInfoIndexRepository.GetAsync(chainId + "-" + symbol);
        tokenInfoIndexData.BlockHeight.ShouldBe(blockHeight);
        tokenInfoIndexData.Decimals.ShouldBe(decimals);
        tokenInfoIndexData.Symbol.ShouldBe(symbol);
        tokenInfoIndexData.TokenName.ShouldBe(tokenName);
        tokenInfoIndexData.TotalSupply.ShouldBe(totalSupply);
        tokenInfoIndexData.Decimals.ShouldBe(decimals);
        tokenInfoIndexData.IsBurnable.ShouldBe(isBurnable);
        tokenInfoIndexData.IssueChainId.ShouldBe(issueChainId);
    }

    [Fact]
    public async Task HandleNFTItemCreatedEventAsync_Test()
    {
        const string symbol = "READ-1";
        const string tokenName = "READ Token";
        const long totalSupply = 0;
        const int decimals = 8;
        const bool isBurnable = true;
        const int issueChainId = 9992731;

        var tokenCreatedProcessor = GetRequiredService<TokenCreatedProcessor>();
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        var tokenCreated = new TokenCreated()
        {
            Symbol = symbol,
            TokenName = tokenName,
            TotalSupply = totalSupply,
            Decimals = decimals,
            Issuer = Address.FromPublicKey("AAA".HexToByteArray()),
            IsBurnable = isBurnable,
            IssueChainId = issueChainId,
            ExternalInfo = new ExternalInfo()
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(tokenCreated.ToLogEvent());
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

        var tokenInfoIndexData = await _nftInfoIndexRepository.GetAsync(chainId + "-" + symbol);
        tokenInfoIndexData.BlockHeight.ShouldBe(blockHeight);
        tokenInfoIndexData.Decimals.ShouldBe(decimals);
        tokenInfoIndexData.Symbol.ShouldBe(symbol);
        tokenInfoIndexData.TokenName.ShouldBe(tokenName);
        tokenInfoIndexData.TotalSupply.ShouldBe(totalSupply);
        tokenInfoIndexData.Decimals.ShouldBe(decimals);
        tokenInfoIndexData.IsBurnable.ShouldBe(isBurnable);
        tokenInfoIndexData.IssueChainId.ShouldBe(issueChainId);
    }


    [Fact]
    public async Task HandleNFTItemCreated_ExternalInfo_Default_EventAsync_Test()
    {
        const string symbol = "READ-1";
        const string tokenName = "READ Token";
        const long totalSupply = 0;
        const int decimals = 8;
        const bool isBurnable = true;
        const int issueChainId = 9992731;

        var tokenCreatedProcessor = GetRequiredService<TokenCreatedProcessor>();
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        var tokenCreated = new TokenCreated()
        {
            Symbol = symbol,
            TokenName = tokenName,
            TotalSupply = totalSupply,
            Decimals = decimals,
            Issuer = Address.FromPublicKey("AAA".HexToByteArray()),
            IsBurnable = isBurnable,
            IssueChainId = issueChainId
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(tokenCreated.ToLogEvent());
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

        var tokenInfoIndexData = await _nftInfoIndexRepository.GetAsync(chainId + "-" + symbol);
        tokenInfoIndexData.BlockHeight.ShouldBe(blockHeight);
        tokenInfoIndexData.Decimals.ShouldBe(decimals);
        tokenInfoIndexData.Symbol.ShouldBe(symbol);
        tokenInfoIndexData.TokenName.ShouldBe(tokenName);
        tokenInfoIndexData.TotalSupply.ShouldBe(totalSupply);
        tokenInfoIndexData.Decimals.ShouldBe(decimals);
        tokenInfoIndexData.IsBurnable.ShouldBe(isBurnable);
        tokenInfoIndexData.IssueChainId.ShouldBe(issueChainId);
    }

    [Fact]
    public async Task HandleNFTItemCreated_ExternalInfo_Null_EventAsync_Test()
    {
        const string symbol = "READ-1";
        const string tokenName = "READ Token";
        const long totalSupply = 0;
        const int decimals = 8;
        const bool isBurnable = true;
        const int issueChainId = 9992731;

        var tokenCreatedProcessor = GetRequiredService<TokenCreatedProcessor>();
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        var tokenCreated = new TokenCreated()
        {
            Symbol = symbol,
            TokenName = tokenName,
            TotalSupply = totalSupply,
            Decimals = decimals,
            Issuer = Address.FromPublicKey("AAA".HexToByteArray()),
            IsBurnable = isBurnable,
            IssueChainId = issueChainId,
            ExternalInfo = null
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(tokenCreated.ToLogEvent());
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

        var tokenInfoIndexData = await _nftInfoIndexRepository.GetAsync(chainId + "-" + symbol);
        tokenInfoIndexData.BlockHeight.ShouldBe(blockHeight);
        tokenInfoIndexData.Decimals.ShouldBe(decimals);
        tokenInfoIndexData.Symbol.ShouldBe(symbol);
        tokenInfoIndexData.TokenName.ShouldBe(tokenName);
        tokenInfoIndexData.TotalSupply.ShouldBe(totalSupply);
        tokenInfoIndexData.Decimals.ShouldBe(decimals);
        tokenInfoIndexData.IsBurnable.ShouldBe(isBurnable);
        tokenInfoIndexData.IssueChainId.ShouldBe(issueChainId);
    }

    [Fact]
    public async Task HandleTokenIssueLogEventAsync_Test()
    {
        const string symbol = "READ";
        const long amount = 10;
        var (caHolderCreated, blockStateSetKey) = await CreateHolder();
        await HandleTokenCreatedAsync_Test();
        var tokenIssuedLogEventProcessor = GetRequiredService<TokenIssuedLogEventProcessor>();
        tokenIssuedLogEventProcessor.GetContractAddress(chainId);

        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        var issued = new Issued()
        {
            To = caHolderCreated.CaAddress,
            Symbol = symbol,
            Amount = amount,
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(issued.ToLogEvent());
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

        await tokenIssuedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await Task.Delay(2000);

        var tokenBalanceIndexData =
            await _caHolderTokenBalanceIndexRepository.GetAsync(chainId + "-" +
                                                                caHolderCreated.CaAddress.ToString()
                                                                    .Trim(new char[] { '"' }) + "-" + symbol);
        tokenBalanceIndexData.BlockHeight.ShouldBe(blockHeight);
    }

    [Fact]
    public async Task HandleNFTCollectionIssueLogEventAsync_Test()
    {
        const string symbol = "READ-0";
        const long amount = 10;
        var (caHolderCreated, blockStateSetKey) = await CreateHolder();
        await HandleNFTCollectionCreatedAsync_Test();
        var tokenIssuedLogEventProcessor = GetRequiredService<TokenIssuedLogEventProcessor>();

        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        var issued = new Issued()
        {
            To = caHolderCreated.CaAddress,
            Symbol = symbol,
            Amount = amount
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(issued.ToLogEvent());
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

        await tokenIssuedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await Task.Delay(2000);

        var tokenBalanceIndexData =
            await _caHolderNFTCollectionBalanceRepository.GetAsync(chainId + "-" +
                                                                   caHolderCreated.CaAddress.ToString()
                                                                       .Trim(new char[] { '"' }) + "-" + symbol);
        tokenBalanceIndexData.BlockHeight.ShouldBe(blockHeight);
        tokenBalanceIndexData.Balance = amount;
    }

    [Fact]
    public async Task HandleNFTItemIssueLogEventAsync_Test()
    {
        const string symbol = "READ-1";
        const long amount = 10;
        var (caHolderCreated, blockStateSetKey) = await CreateHolder();
        await HandleNFTCollectionIssueLogEventAsync_Test();
        await HandleNFTItemCreatedEventAsync_Test();
        var tokenIssuedLogEventProcessor = GetRequiredService<TokenIssuedLogEventProcessor>();

        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        var issued = new Issued()
        {
            To = caHolderCreated.CaAddress,
            Symbol = symbol,
            Amount = amount,
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(issued.ToLogEvent());
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

        await tokenIssuedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await Task.Delay(2000);

        var tokenBalanceIndexData =
            await _caHolderNFTBalanceIndexRepository.GetAsync(chainId + "-" +
                                                              caHolderCreated.CaAddress.ToString()
                                                                  .Trim(new char[] { '"' }) + "-" + symbol);
        tokenBalanceIndexData.BlockHeight.ShouldBe(blockHeight);
        tokenBalanceIndexData.Balance = amount;
    }

    [Fact]
    public async Task HandleTokenTransferredAsync_Test()
    {
        var (holderA, _) = await CreateHolder();
        var (holderB, _) = await CreateHolder(email: holderBEmail, caaddressB, creatorB, managerB, chainId);
        const string symbol = "READ";
        const long amount = 1;
        await HandleTokenIssueLogEventAsync_Test();
        var tokenTransferredLogEventProcessor = GetRequiredService<TokenTransferredLogEventProcessor>();
        tokenTransferredLogEventProcessor.GetContractAddress(chainId);
        var tokenTransferredProcessor = GetRequiredService<TokenTransferredProcessor>();
        tokenTransferredProcessor.GetContractAddress(chainId);

        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetTransfer = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);
        var blockStateSetKeyTransfer = await InitializeBlockStateSetAsync(blockStateSetTransfer, chainId);

        var transferred = new Transferred()
        {
            To = holderB.CaAddress,
            From = holderA.CaAddress,
            Symbol = symbol,
            Amount = amount,
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(transferred.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId = chainId;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        var logEventContext = new LogEventContext
        {
            To = "CAAddress",
            ChainId = chainId,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId,
            MethodName = transferMethodName,
            BlockTime = DateTime.UtcNow,
            ExtraProperties = extraProperties
        };
        await tokenTransferredProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await tokenTransferredLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransfer);

        await Task.Delay(2000);

        //step5: check result
        var tokenBalanceIndexData =
            await _caHolderTokenBalanceIndexRepository.GetAsync(chainId + "-" +
                                                                holderB.CaAddress.ToString()
                                                                    .Trim(new char[] { '"' }) + "-" + symbol);
    }

    [Fact]
    public async Task HandleNFTCollectionTransferredAsync_Test()
    {
        var (holderA, _) = await CreateHolder();
        var (holderB, _) = await CreateHolder(email: holderBEmail, caaddressB, creatorB, managerB, chainIdSide);
        const string symbol = "READ-0";
        const long amount = 1;
        await HandleNFTCollectionIssueLogEventAsync_Test();
        var tokenTransferredLogEventProcessor = GetRequiredService<TokenTransferredLogEventProcessor>();
        var tokenTransferredProcessor = GetRequiredService<TokenTransferredProcessor>();

        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetTransfer = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);
        var blockStateSetKeyTransfer = await InitializeBlockStateSetAsync(blockStateSetTransfer, chainId);

        var transferred = new Transferred()
        {
            To = holderB.CaAddress,
            From = holderA.CaAddress,
            Symbol = symbol,
            Amount = amount,
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(transferred.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId = chainId;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        var logEventContext = new LogEventContext
        {
            To = "CAAddress",
            ChainId = chainId,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId,
            MethodName = transferMethodName,
            BlockTime = DateTime.UtcNow,
            ExtraProperties = extraProperties
        };
        await tokenTransferredProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await tokenTransferredLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransfer);
        await Task.Delay(2000);

        //step5: check result
        var tokenBalanceIndexData =
            await _caHolderNFTCollectionBalanceRepository.GetAsync(chainId + "-" +
                                                                   holderB.CaAddress.ToString()
                                                                       .Trim(new char[] { '"' }) + "-" + symbol);
        // tokenBalanceIndexData.BlockHeight.ShouldBe(blockHeight);
    }

    [Fact]
    public async Task HandleNFTItemTransferredAsync_Test()
    {
        var (holderA, _) = await CreateHolder();
        var (holderB, _) = await CreateHolder(email: holderBEmail, caaddressB, creatorB, managerB, chainIdSide);
        const string symbol = "READ-1";
        const long amount = 10;
        await HandleNFTCollectionIssueLogEventAsync_Test();
        await HandleNFTItemIssueLogEventAsync_Test();
        var tokenTransferredLogEventProcessor = GetRequiredService<TokenTransferredLogEventProcessor>();
        var tokenTransferredProcessor = GetRequiredService<TokenTransferredProcessor>();

        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetTransfer = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);
        var blockStateSetKeyTransfer = await InitializeBlockStateSetAsync(blockStateSetTransfer, chainId);

        var transferred = new Transferred()
        {
            To = holderB.CaAddress,
            From = holderA.CaAddress,
            Symbol = symbol,
            Amount = amount,
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(transferred.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId = chainId;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        var logEventContext = new LogEventContext
        {
            To = "CAAddress",
            ChainId = chainId,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId,
            MethodName = transferMethodName,
            BlockTime = DateTime.UtcNow,
            ExtraProperties = extraProperties
        };
        await tokenTransferredProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await tokenTransferredLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransfer);
        await Task.Delay(2000);

        var tokenBalanceIndexData =
            await _caHolderNFTBalanceIndexRepository.GetAsync(chainId + "-" +
                                                              holderB.CaAddress.ToString()
                                                                  .Trim(new char[] { '"' }) + "-" + symbol);
    }

    [Fact]
    public async Task HandleTokenCrossChainTransactionAsync_Test()
    {
        var (holderB, _) = await CreateHolder(email: holderBEmail, caaddressB, creatorB, managerB, chainIdSide);
        const string symbol = "READ";
        const long amount = 1;
        await HandleTokenIssueLogEventAsync_Test();
        var tokenCrossChainTransferredProcessor = GetRequiredService<TokenCrossChainTransferredProcessor>();
        tokenCrossChainTransferredProcessor.GetContractAddress(chainId);
        var blockStateSet = new BlockStateSet<LogEventInfo>
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
        var crossChainTransferred = new CrossChainTransferred()
        {
            From = Address.FromPublicKey(defaultManager.HexToByteArray()),
            To = holderB.CaAddress,
            ToChainId = 1866392,
            Symbol = symbol,
            Amount = amount
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(crossChainTransferred.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId = chainId;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        var logEventContext = new LogEventContext
        {
            To = contractAddress,
            ChainId = chainId,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            BlockTime = DateTime.Now.ToUniversalTime(),
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId,
            MethodName = crossChainTransferMethodName,
            ExtraProperties = new Dictionary<string, string>()
            {
                // { "TransactionFee", "0" }
            }
        };

        //step3: handle event and write result to blockStateSet
        await tokenCrossChainTransferredProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await Task.Delay(2000);
        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

        var tokenBalanceIndexData =
            await _caHolderTokenBalanceIndexRepository.GetAsync(chainIdSide + "-" +
                                                                holderB.CaAddress.ToString()
                                                                    .Trim(new char[] { '"' }) + "-" + symbol);
    }
    
    [Fact]
    public async Task HandleTokenCrossChainTransactionCompatibleAsync_Test()
    {
        var (holderB, _) = await CreateHolder(email: holderBEmail, caaddressB, creatorB, managerB);
        const string symbol = "READ";
        const long amount = 1;
        await HandleTokenIssueLogEventAsync_Test();
        var tokenCrossChainTransferredProcessor = GetRequiredService<TokenCrossChainTransferredProcessor>();
        tokenCrossChainTransferredProcessor.GetContractAddress(chainId);
        var blockStateSet = new BlockStateSet<LogEventInfo>
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
        var crossChainTransferred = new CrossChainTransferred()
        {
            From = Address.FromPublicKey(managerC.HexToByteArray()),
            To = holderB.CaAddress,
            ToChainId = 1866392,
            Symbol = symbol,
            Amount = amount
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(crossChainTransferred.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId = chainId;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        var logEventContext = new LogEventContext
        {
            To = contractAddress,
            ChainId = chainId,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            BlockTime = DateTime.Now.ToUniversalTime(),
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId,
            MethodName = crossChainTransferMethodName,
            ExtraProperties = new Dictionary<string, string>()
            {
                // { "TransactionFee", "0" }
            }
        };

        //step3: handle event and write result to blockStateSet
        await tokenCrossChainTransferredProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await Task.Delay(2000);
        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);

    }

    [Fact]
    public async Task HandleNFTCollectionCrossChainTransactionAsync_Test()
    {
        var (holderB, _) = await CreateHolder(email: holderBEmail, caaddressB, creatorB, managerB, chainIdSide);
        const string symbol = "READ-0";
        const long amount = 1;
        await HandleNFTCollectionIssueLogEventAsync_Test();
        var tokenCrossChainTransferredProcessor = GetRequiredService<TokenCrossChainTransferredProcessor>();
        var blockStateSet = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainIdSide);
        var crossChainTransferred = new CrossChainTransferred()
        {
            From = Address.FromPublicKey(defaultManager.HexToByteArray()),
            To = holderB.CaAddress,
            Symbol = symbol,
            Amount = amount
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(crossChainTransferred.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId = chainId;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        var logEventContext = new LogEventContext
        {
            To = contractAddress,
            ChainId = chainId,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId,
            MethodName = crossChainTransferMethodName
        };

        //step3: handle event and write result to blockStateSet
        await tokenCrossChainTransferredProcessor.HandleEventAsync(logEventInfo, logEventContext);

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await Task.Delay(2000);

        var tokenBalanceIndexData =
            await _caHolderTokenBalanceIndexRepository.GetAsync(chainIdSide + "-" +
                                                                holderB.CaAddress.ToString()
                                                                    .Trim(new char[] { '"' }) + "-" + symbol);
    }

    [Fact]
    public async Task HandleNFTItemCrossChainTransactionAsync_Test()
    {
        var (holderB, _) = await CreateHolder(email: holderBEmail, caaddressB, creatorB, managerB, chainIdSide);
        const string symbol = "READ-1";
        const long amount = 1;
        await HandleNFTCollectionIssueLogEventAsync_Test();
        await HandleNFTItemIssueLogEventAsync_Test();
        var tokenCrossChainTransferredProcessor = GetRequiredService<TokenCrossChainTransferredProcessor>();
        var blockStateSet = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainIdSide);
        var crossChainTransferred = new CrossChainTransferred()
        {
            From = Address.FromPublicKey(defaultManager.HexToByteArray()),
            To = holderB.CaAddress,
            Symbol = symbol,
            Amount = amount
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(crossChainTransferred.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId = chainId;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        var logEventContext = new LogEventContext
        {
            To = contractAddress,
            ChainId = chainId,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId,
            MethodName = crossChainTransferMethodName
        };

        await tokenCrossChainTransferredProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await Task.Delay(2000);

        var tokenBalanceIndexData =
            await _caHolderTokenBalanceIndexRepository.GetAsync(chainIdSide + "-" +
                                                                holderB.CaAddress.ToString()
                                                                    .Trim(new char[] { '"' }) + "-" + symbol);
    }

    [Fact]
    public async Task HandleTokenCrossChainReceivedLogEventAsync_Test()
    {
        var (holderA, _) = await CreateHolder();
        var (holderB, _) = await CreateHolder(email: holderBEmail, caaddressB, creatorB, managerB, chainIdSide);
        const string symbol = "READ";
        const long amount = 1;
        await HandleTokenCrossChainTransactionAsync_Test();
        var tokenCrossChainReceivedLogEventProcessor = GetRequiredService<TokenCrossChainReceivedLogEventProcessor>();
        tokenCrossChainReceivedLogEventProcessor.GetContractAddress(chainId);
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        //step1: create blockStateSet
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainIdSide);
        //step2: create logEventInfo
        var crossChainReceived = new CrossChainReceived()
        {
            From = Address.FromPublicKey(defaultManager.HexToByteArray()),
            To = holderB.CaAddress,
            Symbol = symbol,
            Amount = amount
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(crossChainReceived.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId = chainIdSide;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        var logEventContext = new LogEventContext
        {
            To = contractAddress,
            ChainId = chainIdSide,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId,
            MethodName = crossChainReceivedMethodName,
        };

        await tokenCrossChainReceivedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await Task.Delay(2000);

        var tokenBalanceIndexData =
            await _caHolderTokenBalanceIndexRepository.GetAsync(chainIdSide + "-" +
                                                                holderB.CaAddress.ToString()
                                                                    .Trim(new char[] { '"' }) + "-" + symbol);
    }

    [Fact]
    public async Task HandleNFTCollectionCrossChainReceivedLogEventAsync_Test()
    {
        var (holderB, _) = await CreateHolder(email: holderBEmail, caaddressB, creatorB, managerB, chainIdSide);
        const string symbol = "READ-0";
        const long amount = 1;
        await HandleNFTCollectionCrossChainTransactionAsync_Test();
        var tokenCrossChainReceivedLogEventProcessor = GetRequiredService<TokenCrossChainReceivedLogEventProcessor>();
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        //step1: create blockStateSet
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainIdSide);
        //step2: create logEventInfo
        var crossChainReceived = new CrossChainReceived()
        {
            From = Address.FromPublicKey(defaultManager.HexToByteArray()),
            To = holderB.CaAddress,
            Symbol = symbol,
            Amount = amount
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(crossChainReceived.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId = chainIdSide;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        var logEventContext = new LogEventContext
        {
            To = contractAddress,
            ChainId = chainIdSide,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId,
            MethodName = crossChainReceivedMethodName
        };

        await tokenCrossChainReceivedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await Task.Delay(2000);

        var tokenBalanceIndexData =
            await _caHolderNFTCollectionBalanceRepository.GetAsync(chainIdSide + "-" +
                                                                   holderB.CaAddress.ToString()
                                                                       .Trim(new char[] { '"' }) + "-" + symbol);
    }

    [Fact]
    public async Task HandleNFTItemCrossChainReceivedLogEventAsync_Test()
    {
        var (holderB, _) = await CreateHolder(email: holderBEmail, caaddressB, creatorB, managerB, chainIdSide);
        const string symbol = "READ-1";
        const long amount = 1;
        await HandleNFTItemCrossChainTransactionAsync_Test();
        var tokenCrossChainReceivedLogEventProcessor = GetRequiredService<TokenCrossChainReceivedLogEventProcessor>();
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainIdSide);
        var crossChainReceived = new CrossChainReceived()
        {
            From = Address.FromPublicKey(defaultManager.HexToByteArray()),
            To = holderB.CaAddress,
            Symbol = symbol,
            Amount = amount
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(crossChainReceived.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId = chainIdSide;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        var logEventContext = new LogEventContext
        {
            To = contractAddress,
            ChainId = chainIdSide,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId,
            MethodName = crossChainReceivedMethodName
        };

        await tokenCrossChainReceivedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await Task.Delay(2000);

        var tokenBalanceIndexData =
            await _caHolderNFTBalanceIndexRepository.GetAsync(chainIdSide + "-" +
                                                              holderB.CaAddress.ToString()
                                                                  .Trim(new char[] { '"' }) + "-" + symbol);
    }

    [Fact]
    public async Task HandleTokenCrossChainReceivedProcessorAsync_Test()
    {
        var (holderA, _) = await CreateHolder();
        var (holderB, _) = await CreateHolder(email: holderBEmail, caaddressB, creatorB, managerB, chainIdSide);
        const string symbol = "READ";
        const long amount = 1;
        await HandleTokenCrossChainTransactionAsync_Test();
        var tokenCrossChainReceivedProcessor = GetRequiredService<TokenCrossChainReceivedProcessor>();
        tokenCrossChainReceivedProcessor.GetContractAddress(chainId);
        var blockStateSet = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        //step1: create blockStateSet
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainIdSide);
        //step2: create logEventInfo
        var crossChainReceived = new CrossChainReceived()
        {
            From = Address.FromPublicKey(defaultManager.HexToByteArray()),
            To = holderB.CaAddress,
            Symbol = symbol,
            Amount = amount,
            TransferTransactionId = HashHelper.ComputeFrom("transferTransactionId")
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(crossChainReceived.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId = chainIdSide;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        var logEventContext = new LogEventContext
        {
            To = contractAddress,
            From = Address.FromPublicKey(defaultManager.HexToByteArray()).ToString(),
            ChainId = chainIdSide,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId,
            MethodName = crossChainReceivedMethodName,
            BlockTime = DateTime.UtcNow,
            ExtraProperties = extraProperties
        };

        await tokenCrossChainReceivedProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await Task.Delay(2000);

        var tokenBalanceIndexData =
            await _caHolderTokenBalanceIndexRepository.GetAsync(chainIdSide + "-" +
                                                                holderB.CaAddress.ToString()
                                                                    .Trim(new char[] { '"' }) + "-" + symbol);
    }

    [Fact]
    public async Task HandleNFTCollectionCrossChainReceivedProcessorAsync_Test()
    {
        var (holderA, _) = await CreateHolder();
        var (holderB, _) = await CreateHolder(email: holderBEmail, caaddressB, creatorB, managerB, chainIdSide);
        const string symbol = "READ-0";
        const long amount = 1;
        await HandleNFTCollectionCrossChainTransactionAsync_Test();
        var tokenCrossChainReceivedProcessor = GetRequiredService<TokenCrossChainReceivedProcessor>();
        var blockStateSet = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        //step1: create blockStateSet
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainIdSide);
        //step2: create logEventInfo
        var crossChainReceived = new CrossChainReceived()
        {
            From = Address.FromPublicKey(defaultManager.HexToByteArray()),
            To = holderB.CaAddress,
            Symbol = symbol,
            Amount = amount,
            TransferTransactionId = HashHelper.ComputeFrom("transferTransactionId")
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(crossChainReceived.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId = chainIdSide;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        var logEventContext = new LogEventContext
        {
            To = contractAddress,
            ChainId = chainIdSide,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId,
            MethodName = crossChainReceivedMethodName
        };

        await tokenCrossChainReceivedProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await Task.Delay(2000);

        var tokenBalanceIndexData =
            await _caHolderNFTCollectionBalanceRepository.GetAsync(chainIdSide + "-" +
                                                                   holderB.CaAddress.ToString()
                                                                       .Trim(new char[] { '"' }) + "-" + symbol);
    }

    [Fact]
    public async Task HandleNFTItemCrossChainReceivedProcessorAsync_Test()
    {
        var (holderA, _) = await CreateHolder();
        var (holderB, _) = await CreateHolder(email: holderBEmail, caaddressB, creatorB, managerB, chainIdSide);
        const string symbol = "READ-1";
        const long amount = 1;
        await HandleNFTItemCrossChainTransactionAsync_Test();
        var tokenCrossChainReceivedProcessor = GetRequiredService<TokenCrossChainReceivedProcessor>();
        var blockStateSet = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainIdSide);
        var crossChainReceived = new CrossChainReceived()
        {
            From = Address.FromPublicKey(defaultManager.HexToByteArray()),
            To = holderB.CaAddress,
            Symbol = symbol,
            Amount = amount,
            TransferTransactionId = HashHelper.ComputeFrom("transferTransactionId")
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(crossChainReceived.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId = chainIdSide;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        var logEventContext = new LogEventContext
        {
            To = contractAddress,
            ChainId = chainIdSide,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId,
            MethodName = crossChainReceivedMethodName
        };

        await tokenCrossChainReceivedProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKey);
        await Task.Delay(2000);

        var tokenBalanceIndexData =
            await _caHolderNFTBalanceIndexRepository.GetAsync(chainIdSide + "-" +
                                                              holderB.CaAddress.ToString()
                                                                  .Trim(new char[] { '"' }) + "-" + symbol);
    }

    [Fact]
    public async Task HandleTokenBurnedLogEventAsync_Test()
    {
        const string symbol = "READ";
        const long amount = 1;
        var (caHolderCreated, _) = await CreateHolder();
        await HandleTokenIssueLogEventAsync_Test();
        var tokenBurnedLogEventProcessor = GetRequiredService<TokenBurnedLogEventProcessor>();
        tokenBurnedLogEventProcessor.GetContractAddress(chainId);

        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        var burned = new Burned()
        {
            Burner = caHolderCreated.CaAddress,
            Symbol = symbol,
            Amount = amount,
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(burned.ToLogEvent());
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

        await tokenBurnedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await Task.Delay(2000);

        var tokenBalanceIndexData =
            await _caHolderTokenBalanceIndexRepository.GetAsync(chainId + "-" +
                                                                caHolderCreated.CaAddress.ToString()
                                                                    .Trim(new char[] { '"' }) + "-" + symbol);
    }

    [Fact]
    public async Task HandleNFTCollectionBurnedLogEventAsync_Test()
    {
        const string symbol = "READ-0";
        const long amount = 1;
        var (caHolderCreated, _) = await CreateHolder();
        await HandleNFTCollectionIssueLogEventAsync_Test();
        var tokenBurnedLogEventProcessor = GetRequiredService<TokenBurnedLogEventProcessor>();


        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        var burned = new Burned()
        {
            Burner = caHolderCreated.CaAddress,
            Symbol = symbol,
            Amount = amount,
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(burned.ToLogEvent());
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

        await tokenBurnedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await Task.Delay(2000);

        var tokenBalanceIndexData =
            await _caHolderNFTCollectionBalanceRepository.GetAsync(chainId + "-" +
                                                                   caHolderCreated.CaAddress.ToString()
                                                                       .Trim(new char[] { '"' }) + "-" + symbol);
    }

    [Fact]
    public async Task HandleNFTItemBurnedLogEventAsync_Test()
    {
        const string symbol = "READ-1";
        const long amount = 1;
        var (caHolderCreated, _) = await CreateHolder();
        await HandleNFTItemIssueLogEventAsync_Test();
        var tokenBurnedLogEventProcessor = GetRequiredService<TokenBurnedLogEventProcessor>();


        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);

        var burned = new Burned()
        {
            Burner = caHolderCreated.CaAddress,
            Symbol = symbol,
            Amount = amount,
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(burned.ToLogEvent());
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

        await tokenBurnedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await Task.Delay(2000);

        var tokenBalanceIndexData =
            await _caHolderNFTBalanceIndexRepository.GetAsync(chainId + "-" +
                                                              caHolderCreated.CaAddress.ToString()
                                                                  .Trim(new char[] { '"' }) + "-" + symbol);
    }

    [Fact]
    public async Task HandleCrossChainTransferredTotalAsync_Test()
    {
        var (holderA, blockStateSetKeyA) = await CreateHolder();
        var (holderB, _) = await CreateHolder(email: holderBEmail, caaddressB, creatorB, managerB);
        var tokenCreatedProcessor = GetRequiredService<TokenCreatedProcessor>();
        var tokenIssuedLogEventProcessor = GetRequiredService<TokenIssuedLogEventProcessor>();
        var tokenCrossChainTransferredProcessor = GetRequiredService<TokenCrossChainTransferredProcessor>();
        var tokenCrossChainReceivedProcessor = GetRequiredService<TokenCrossChainReceivedProcessor>();
        var tokenCrossChainReceivedLogEventProcessor = GetRequiredService<TokenCrossChainReceivedLogEventProcessor>();
        var tokenBurnedLogEventProcessor = GetRequiredService<TokenBurnedLogEventProcessor>();

        const string symbol = "READ";
        const string tokenName = "READ Token";
        const long totalSupply = 0;
        const int decimals = 8;
        const bool isBurnable = true;
        const int amount = 10;
        const int issueChainId = 9992731;

        var tokenCreated = new TokenCreated()
        {
            Symbol = symbol,
            TokenName = tokenName,
            TotalSupply = totalSupply,
            Decimals = decimals,
            Issuer = Address.FromPublicKey("AAA".HexToByteArray()),
            IsBurnable = isBurnable,
            IssueChainId = issueChainId,
            ExternalInfo = new ExternalInfo()
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(tokenCreated.ToLogEvent());
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
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKeyA);
        await Task.Delay(2000);

        var issued = new Issued()
        {
            To = holderA.CaAddress,
            Symbol = symbol,
            Amount = amount,
        };

        logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(issued.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId = chainId;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        logEventContext = new LogEventContext
        {
            ChainId = chainId,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId
        };

        await tokenIssuedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKeyA);
        await Task.Delay(2000);

        /*
         *  cross chain handling
         */
        var blockStateSetCross = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash
        };

        var blockStateSetReceived = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash
        };

        var blockStateSetTransfer = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash
        };

        var blockStateSetKeyCross = await InitializeBlockStateSetAsync(blockStateSetCross, chainId);
        var blockStateSetKeyTransfer = await InitializeBlockStateSetAsync(blockStateSetTransfer, chainId);
        var blockStateSetKeyReceived = await InitializeBlockStateSetAsync(blockStateSetReceived, chainIdSide);

        var crossChainTransferred = new CrossChainTransferred()
        {
            From = holderA.CaAddress,
            To = holderB.CaAddress,
        };
        var crossChainReceived = new CrossChainReceived()
        {
            From = holderA.CaAddress,
            To = holderB.CaAddress,
        };

        logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(crossChainTransferred.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId = chainId;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        logEventContext = new LogEventContext
        {
            // To = holderB.CaAddress.ToString(),
            // From = holderA.CaAddress.ToString(),
            ChainId = chainId,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId
        };
        await tokenCrossChainTransferredProcessor.HandleEventAsync(logEventInfo, logEventContext);

        logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(crossChainReceived.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId = chainIdSide;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        logEventContext = new LogEventContext
        {
            // To = holderB.CaAddress.ToString(),
            // From = holderA.CaAddress.ToString(),
            ChainId = chainIdSide,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId
        };
        await tokenCrossChainReceivedProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await tokenCrossChainReceivedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);

        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKeyCross);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransfer);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyReceived);
        await Task.Delay(2000);

        var blockStateSetBurned = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash
        };
        var blockStateSetKeyBurned = await InitializeBlockStateSetAsync(blockStateSetBurned, chainIdSide);
        var burned = new Burned()
        {
            Amount = 1,
            Burner = holderB.CaAddress,
            Symbol = symbol,
        };
        logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(burned.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId = chainIdSide;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        logEventContext = new LogEventContext
        {
            ChainId = chainIdSide,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId
        };

        await tokenBurnedLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKeyBurned);
        await Task.Delay(2000);
    }

    [Fact]
    public async Task QueryCAHolderNFTCollectionBalanceInfoTests()
    {
        await HandleNFTItemIssueLogEventAsync_Test();

        var result = await Query.CAHolderNFTCollecitonBalanceInfo(_caHolderNFTCollectionBalanceRepository,
            _objectMapper, new GetCAHolderNFTCollectionInfoDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                ChainId = "AELF",
                CAAddressInfos = new List<CAAddressInfo>
                {
                    new()
                    {
                        CAAddress = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
                        ChainId = "AELF"
                    }
                },
                Symbol = "READ-0"
            });
        result.TotalRecordCount.ShouldBe(1);
        result.Data.Count.ShouldBe(1);
        result.Data.First().CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        result.Data.First().ChainId.ShouldBe("AELF");
        result.Data.First().TokenIds.Count.ShouldBe(1);
        result.Data.First().NftCollectionInfo.Decimals.ShouldBe(8);
        result.Data.First().NftCollectionInfo.TokenName.ShouldBe("READ Token");
        result.Data.First().NftCollectionInfo.TokenContractAddress.ShouldBe("token");
        result.Data.First().NftCollectionInfo.Symbol.ShouldBe("READ-0");
    }

    [Fact]
    public async Task QueryCAHolderNFTBalanceInfoTests()
    {
        await HandleNFTItemIssueLogEventAsync_Test();

        var result = await Query.CAHolderNFTBalanceInfo(_caHolderNFTBalanceIndexRepository,
            _objectMapper, new GetCAHolderNFTInfoDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                ChainId = "AELF",
                CAAddressInfos = new List<CAAddressInfo>
                {
                    new()
                    {
                        CAAddress = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
                        ChainId = "AELF"
                    }
                },
                CollectionSymbol = "READ-0",
                Symbol = "READ-1"
            });
        result.TotalRecordCount.ShouldBe(1);
        result.Data.Count.ShouldBe(1);
        result.Data.First().CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        result.Data.First().ChainId.ShouldBe("AELF");
        result.Data.First().Balance.ShouldBe(10);
        result.Data.First().NftInfo.Decimals.ShouldBe(8);
        result.Data.First().NftInfo.TokenName.ShouldBe("READ Token");
        result.Data.First().NftInfo.CollectionName.ShouldBe("READ Token");
        result.Data.First().NftInfo.CollectionSymbol.ShouldBe("READ-0");
        result.Data.First().NftInfo.Symbol.ShouldBe("READ-1");
        result.Data.First().NftInfo.IsBurnable.ShouldBeTrue();
    }

    [Fact]
    public async Task QueryTokenInfoTest()
    {
        await HandleTokenCreatedAsync_Test();

        var result = await Query.TokenInfo(_tokenInfoIndexRepository, _objectMapper, new GetTokenInfoDto
        {
            SkipCount = 0,
            MaxResultCount = 10,
            ChainId = "AELF",
            Symbol = "READ"
        });
        result.Count.ShouldBe(1);
        result.First().Decimals.ShouldBe(8);
        result.First().TokenName.ShouldBe("READ Token");
    }

    [Fact]
    public async Task QueryTokenInfo_Symbol_Fuzzy_Matching_Test()
    {
        await HandleTokenCreatedAsync_Test();

        var result = await Query.TokenInfo(_tokenInfoIndexRepository, _objectMapper, new GetTokenInfoDto
        {
            SkipCount = 0,
            MaxResultCount = 10,
            ChainId = "AELF",
            SymbolKeyword = "EA"
        });
        result.Count.ShouldBe(1);
        result.First().Decimals.ShouldBe(8);
        result.First().TokenName.ShouldBe("READ Token");
    }

    [Fact]
    public async Task QueryCAHolderTransactionTest()
    {
        await HandleTokenCrossChainTransactionAsync_Test();

        var result = await Query.CAHolderTransaction(_caHolderTransactionIndexRepository, transactionFeeRepository, _objectMapper,
            new GetCAHolderTransactionDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                ChainId = chainIdSide,
                CAAddressInfos = new List<CAAddressInfo>
                {
                    new()
                    {
                        CAAddress = Address.FromPublicKey("AAAA".HexToByteArray()).ToBase58(),
                        ChainId = chainIdSide
                    }
                },
                Symbol = "READ",
                MethodNames = new List<string>
                {
                    "Transferred",
                    "Transfer",
                    "CrossChainTransfer"
                }
            });

        result.TotalRecordCount.ShouldBe(1);
        result.Data.Count.ShouldBe(1);
        result.Data.FirstOrDefault().MethodName.ShouldBe("CrossChainTransfer");
        result.Data.FirstOrDefault().TransferInfo.FromCAAddress
            .ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        result.Data.FirstOrDefault().TransferInfo.ToAddress
            .ShouldBe(Address.FromPublicKey("AAAA".HexToByteArray()).ToBase58());
        result.Data.FirstOrDefault().TransferInfo.Amount.ShouldBe(1);
        result.Data.FirstOrDefault().TransferInfo.ToChainId.ShouldBe(chainIdSide);
        result.Data.FirstOrDefault().TokenInfo.Symbol.ShouldBe("READ");
    }

    [Fact]
    public async Task QueryCAHolderTransaction_StartBlockHeight_EndBlockHeight_Test()
    {
        await HandleTokenCrossChainTransactionAsync_Test();

        var result = await Query.CAHolderTransaction(_caHolderTransactionIndexRepository, transactionFeeRepository, _objectMapper,
            new GetCAHolderTransactionDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                ChainId = chainIdSide,
                StartBlockHeight = 1,
                EndBlockHeight = 1000000000,
                CAAddressInfos = new List<CAAddressInfo>
                {
                    new()
                    {
                        CAAddress = Address.FromPublicKey("AAAA".HexToByteArray()).ToBase58(),
                        ChainId = chainIdSide
                    }
                },
                Symbol = "READ",
                MethodNames = new List<string>
                {
                    "Transferred",
                    "Transfer",
                    "CrossChainTransfer"
                }
            });

        result.TotalRecordCount.ShouldBe(1);
        result.Data.Count.ShouldBe(1);
        result.Data.FirstOrDefault().MethodName.ShouldBe("CrossChainTransfer");
        result.Data.FirstOrDefault().TransferInfo.FromCAAddress
            .ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        result.Data.FirstOrDefault().TransferInfo.ToAddress
            .ShouldBe(Address.FromPublicKey("AAAA".HexToByteArray()).ToBase58());
        result.Data.FirstOrDefault().TransferInfo.Amount.ShouldBe(1);
        result.Data.FirstOrDefault().TransferInfo.ToChainId.ShouldBe(chainIdSide);
        result.Data.FirstOrDefault().TokenInfo.Symbol.ShouldBe("READ");
    }

    [Fact]
    public async Task QueryTwoCAHolderTransactionTest()
    {
        await HandleTokenCrossChainTransactionAsync_Test();

        var result = await Query.TwoCAHolderTransaction(_caHolderTransactionIndexRepository, _objectMapper,
            new GetTwoCAHolderTransactionDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                CAAddressInfos = new List<CAAddressInfo>
                {
                    new()
                    {
                        CAAddress = Address.FromPublicKey("AAAA".HexToByteArray()).ToBase58(),
                        ChainId = chainIdSide
                    },
                    new()
                    {
                        CAAddress = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
                        ChainId = ""
                    }
                },
                Symbol = "READ",
                MethodNames = new List<string>
                {
                    "Transferred",
                    "Transfer",
                    "CrossChainTransfer"
                }
            });

        result.TotalRecordCount.ShouldBe(1);
        result.Data.Count.ShouldBe(1);
        result.Data.FirstOrDefault().MethodName.ShouldBe("CrossChainTransfer");
        result.Data.FirstOrDefault().TransferInfo.Amount.ShouldBe(1);
        result.Data.FirstOrDefault().TokenInfo.Symbol.ShouldBe("READ");
    }

    [Fact]
    public async Task QueryTwoCAHolderTransaction_StartBlockHeight_EndBlockHeight_Test()
    {
        await HandleTokenCrossChainTransactionAsync_Test();

        var result = await Query.TwoCAHolderTransaction(_caHolderTransactionIndexRepository, _objectMapper,
            new GetTwoCAHolderTransactionDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                StartBlockHeight = 1,
                EndBlockHeight = 10000000000,
                TransactionId = string.Empty,
                TransferTransactionId = string.Empty,
                CAAddressInfos = new List<CAAddressInfo>
                {
                    new()
                    {
                        CAAddress = Address.FromPublicKey("AAAA".HexToByteArray()).ToBase58(),
                        ChainId = chainIdSide
                    },
                    new()
                    {
                        CAAddress = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
                        ChainId = ""
                    }
                },
                Symbol = "READ",
                MethodNames = new List<string>
                {
                    "Transferred",
                    "Transfer",
                    "CrossChainTransfer"
                }
            });

        result.TotalRecordCount.ShouldBe(1);
        result.Data.Count.ShouldBe(1);
        result.Data.FirstOrDefault().MethodName.ShouldBe("CrossChainTransfer");
        result.Data.FirstOrDefault().TransferInfo.Amount.ShouldBe(1);
        result.Data.FirstOrDefault().TokenInfo.Symbol.ShouldBe("READ");
    }

    [Fact]
    public async Task QueryCAHolderTransactionInfoTest()
    {
        await HandleTokenTransferredAsync_Test();

        var result = await Query.CAHolderTransactionInfo(_caHolderTransactionIndexRepository, _compatibleCrossChainTransferRepository, _objectMapper,
            new GetCAHolderTransactionInfoDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                ChainId = chainId,
                CAAddresses = new List<string>
                {
                    Address.FromPublicKey("AAAA".HexToByteArray()).ToBase58()
                },
                Symbol = "READ",
                MethodNames = new List<string>
                {
                    "Transferred",
                    "Transfer",
                    "CrossChainTransfer"
                }
            });
        result.TotalRecordCount.ShouldBe(1);
        result.Data.Count.ShouldBe(1);
        result.Data.FirstOrDefault().MethodName.ShouldBe("Transferred");
        result.Data.FirstOrDefault().TransferInfo.FromCAAddress
            .ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        result.Data.FirstOrDefault().ChainId.ShouldBe("AELF");
        result.Data.FirstOrDefault().TransferInfo.Amount.ShouldBe(1);

        await HandleTokenCrossChainTransactionCompatibleAsync_Test();
        result = await Query.CAHolderTransactionInfo(_caHolderTransactionIndexRepository, _compatibleCrossChainTransferRepository, _objectMapper,
            new GetCAHolderTransactionInfoDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                ChainId = chainId,
                // CAAddresses = new List<string>
                // {
                //     Address.FromPublicKey("AAAA".HexToByteArray()).ToBase58()
                // },
                Symbol = "READ",
                MethodNames = new List<string>
                {
                    "Transferred",
                    "Transfer",
                    "CrossChainTransfer"
                }
            });
        result.TotalRecordCount.ShouldBe(1);
    }

    [Fact]
    public async Task Query_CAHolderTokenBalance_Test()
    {
        var (holder, _) = await CreateHolder();
        await HandleTokenIssueLogEventAsync_Test();
        await Task.Delay(1000);
        var param = new GetCAHolderTokenBalanceDto()
        {
            ChainId = chainId,
            Symbol = defaultSymbol,
            CAAddressInfos = new List<CAAddressInfo>
            {
                new()
                {
                    CAAddress = holder.CaAddress.ToString().Trim(new char[] { '"' }),
                    ChainId = "AELF"
                }
            },
            SkipCount = 0,
            MaxResultCount = 10,
        };
        var result = await Query.CAHolderTokenBalanceInfo(_caHolderTokenBalanceIndexRepository, _objectMapper, param);
        result.Data.Count.ShouldBe(1);
        result.Data.FirstOrDefault().CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
    }

    [Fact]
    public async Task Query_CAHolderSearchToken_Test()
    {
        var (holder, _) = await CreateHolder();
        await HandleNFTItemIssueLogEventAsync_Test();
        await Task.Delay(1000);
        var param = new GetCAHolderSearchTokenNFTDto()
        {
            ChainId = chainId,
            SearchWord = "",
            CAAddressInfos = new List<CAAddressInfo>
            {
                new()
                {
                    CAAddress = holder.CaAddress.ToString().Trim(new char[] { '"' }),
                    ChainId = "AELF"
                }
            },
            SkipCount = 0,
            MaxResultCount = 10,
        };
        var result = await Query.CAHolderSearchTokenNFT(_caHolderSearchTokenNFTIndexRepository, _objectMapper, param);
        result.Data.Count.ShouldBe(1);
        result.Data.FirstOrDefault().CAAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
    }

    // [Fact]
    // public async Task Query_SyncState_Test()
    // {
    //     var aelfIndexerClientInfoProvider = GetRequiredService<IAElfIndexerClientInfoProvider>();
    //     var clusterClient = GetRequiredService<IClusterClient>();
    //     var (holder, _) = await CreateHolder();
    //     await Task.Delay(1000);
    //     var param = new GetSyncStateDto()
    //     {
    //         ChainId = "AELF",
    //         FilterType = 0,
    //     };
    //     var result = await Query.SyncState(clusterClient,aelfIndexerClientInfoProvider, _objectMapper, param);
    // }

    [Fact]
    public async Task HandleNFTCollectionTransferredFromChainAsync_Test()
    {
        var (holderA, _) = await CreateHolder();
        var (holderB, _) = await CreateHolder(email: holderBEmail, caaddressB, creatorB, managerB, chainIdSide);
        const string symbol = "READ-0";
        const long amount = 1;
        var tokenTransferredLogEventProcessor = GetRequiredService<TokenTransferredLogEventProcessor>();
        var tokenTransferredProcessor = GetRequiredService<TokenTransferredProcessor>();

        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetTransfer = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);
        var blockStateSetKeyTransfer = await InitializeBlockStateSetAsync(blockStateSetTransfer, chainId);

        var transferred = new Transferred()
        {
            To = holderB.CaAddress,
            From = holderA.CaAddress,
            Symbol = symbol,
            Amount = amount,
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(transferred.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId = chainId;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        var logEventContext = new LogEventContext
        {
            To = "CAAddress",
            ChainId = chainId,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId,
            MethodName = transferMethodName,
            BlockTime = DateTime.UtcNow,
            ExtraProperties = extraProperties
        };
        await tokenTransferredProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await tokenTransferredLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransfer);
        await Task.Delay(2000);

        //step5: check result
        var nftCollectionInfoIndex =
            await _nftCollectionInfoIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(chainId, symbol),
                chainId);
        nftCollectionInfoIndex.Symbol.ShouldBe(symbol);
        nftCollectionInfoIndex.Supply.ShouldBe(1);
    }

    [Fact]
    public async Task HandleNFTItemTransferredFromChainAsync_Test()
    {
        var (holderA, _) = await CreateHolder();
        var (holderB, _) = await CreateHolder(email: holderBEmail, caaddressB, creatorB, managerB, chainIdSide);
        const string symbol = "READ-1";
        const long amount = 1;
        var tokenTransferredLogEventProcessor = GetRequiredService<TokenTransferredLogEventProcessor>();
        var tokenTransferredProcessor = GetRequiredService<TokenTransferredProcessor>();

        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetTransfer = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);
        var blockStateSetKeyTransfer = await InitializeBlockStateSetAsync(blockStateSetTransfer, chainId);

        var transferred = new Transferred()
        {
            To = holderB.CaAddress,
            From = holderA.CaAddress,
            Symbol = symbol,
            Amount = amount,
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(transferred.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId = chainId;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        var logEventContext = new LogEventContext
        {
            To = "CAAddress",
            ChainId = chainId,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId,
            MethodName = transferMethodName,
            BlockTime = DateTime.UtcNow,
            ExtraProperties = extraProperties
        };
        await tokenTransferredProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await tokenTransferredLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransfer);
        await Task.Delay(2000);

        //step5: check result
        var nftCollectionSymbol = "READ-0";
        var nftCollectionInfoIndex =
            await _nftCollectionInfoIndexRepository.GetFromBlockStateSetAsync(
                IdGenerateHelper.GetId(chainId, nftCollectionSymbol), chainId);
        nftCollectionInfoIndex.Symbol.ShouldBe(nftCollectionSymbol);
        nftCollectionInfoIndex.Supply.ShouldBe(1);

        var nftInfoIndex =
            await _nftInfoIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(chainId, symbol), chainId);
        nftInfoIndex.Symbol.ShouldBe(symbol);
        nftInfoIndex.Supply.ShouldBe(1);
        nftInfoIndex.CollectionName.ShouldBe(nftCollectionSymbol);
        nftInfoIndex.CollectionSymbol.ShouldBe(nftCollectionSymbol);

        var transferTransactionIndex =
            await _caHolderTransactionIndexRepository.GetFromBlockStateSetAsync(
                IdGenerateHelper.GetId(blockHash, transactionId), chainId);
        transferTransactionIndex.TransactionId.ShouldBe(transactionId);
        transferTransactionIndex.TransferInfo.FromAddress.ShouldBe(holderA.CaAddress.ToBase58());
        transferTransactionIndex.TransferInfo.ToAddress.ShouldBe(holderB.CaAddress.ToBase58());
    }

    [Fact]
    public async Task HandleTokenTransferFromChainAsync_Test()
    {
        var (holderA, _) = await CreateHolder();
        var (holderB, _) = await CreateHolder(email: holderBEmail, caaddressB, creatorB, managerB, chainId);
        const string symbol = "READ";
        const long amount = 1;
        var tokenTransferredLogEventProcessor = GetRequiredService<TokenTransferredLogEventProcessor>();
        tokenTransferredLogEventProcessor.GetContractAddress(chainId);
        var tokenTransferredProcessor = GetRequiredService<TokenTransferredProcessor>();
        tokenTransferredProcessor.GetContractAddress(chainId);

        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetTransfer = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash
        };
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);
        var blockStateSetKeyTransfer = await InitializeBlockStateSetAsync(blockStateSetTransfer, chainId);

        var transferred = new Transferred()
        {
            To = holderB.CaAddress,
            From = holderA.CaAddress,
            Symbol = symbol,
            Amount = amount,
        };

        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(transferred.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId = chainId;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        var logEventContext = new LogEventContext
        {
            To = "CAAddress",
            ChainId = chainId,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId,
            MethodName = transferMethodName,
            BlockTime = DateTime.UtcNow,
            ExtraProperties = extraProperties
        };
        await tokenTransferredProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await tokenTransferredLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransfer);

        await Task.Delay(2000);

        //step5: check result
        var tokenBalanceIndexData =
            await _caHolderTokenBalanceIndexRepository.GetAsync(chainId + "-" +
                                                                holderB.CaAddress.ToString()
                                                                    .Trim(new char[] { '"' }) + "-" + symbol);
        tokenBalanceIndexData.Balance.ShouldBe(1);
        var tokenInfoIndex =
            await _tokenInfoIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(chainId, symbol), chainId);
        tokenInfoIndex.Symbol.ShouldBe(symbol);
        tokenInfoIndex.Type.ShouldBe(TokenType.Token);
        var transferTransactionIndex =
            await _caHolderTransactionIndexRepository.GetFromBlockStateSetAsync(
                IdGenerateHelper.GetId(blockHash, transactionId), chainId);
        transferTransactionIndex.TransactionId.ShouldBe(transactionId);
        transferTransactionIndex.TransferInfo.FromAddress.ShouldBe(holderA.CaAddress.ToBase58());
        transferTransactionIndex.TransferInfo.ToAddress.ShouldBe(holderB.CaAddress.ToBase58());
    }
}