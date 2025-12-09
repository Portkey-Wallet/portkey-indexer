using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.BingoGameContract;
using Portkey.Indexer.CA.Entities;
using Portkey.Indexer.CA.GraphQL;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public class BingoedProcessor : CAHolderTransactionProcessorBase<Bingoed>
{   
    
    private readonly IAElfIndexerClientEntityRepository<BingoGameIndex, TransactionInfo> _bingoIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<BingoGameStaticsIndex, TransactionInfo> _bingoStaticsIndexRepository;
    public BingoedProcessor(ILogger<BingoedProcessor> logger,
        IAElfIndexerClientEntityRepository<BingoGameIndex, TransactionInfo> bingoIndexRepository,
        IAElfIndexerClientEntityRepository<BingoGameStaticsIndex, TransactionInfo> bingoStaticsIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderManagerIndex, TransactionInfo> caHolderManagerIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>
            caHolderTransactionIndexRepository,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, TransactionInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, TransactionInfo> nftInfoIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo>
            caHolderTransactionAddressIndexRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions, IObjectMapper objectMapper) :
        base(logger, caHolderIndexRepository,caHolderManagerIndexRepository, caHolderTransactionIndexRepository, tokenInfoIndexRepository,
            nftInfoIndexRepository, caHolderTransactionAddressIndexRepository, contractInfoOptions,
            caHolderTransactionInfoOptions, objectMapper)
    {
        _bingoIndexRepository = bingoIndexRepository;
        _bingoStaticsIndexRepository = bingoStaticsIndexRepository;
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).BingoGameContractAddress;
    }

    protected override async Task HandleEventAsync(Bingoed eventValue, LogEventContext context)
    {
        if (eventValue.PlayerAddress == null || eventValue.PlayerAddress.Value == null)
        {
            return;
        }
        
        // await ProcessCAHolderTransactionAsync(context, eventValue.PlayerAddress.ToBase58());
        if (!IsValidTransaction(context.ChainId, context.To, context.MethodName, context.Params)) return;
        var holder = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            eventValue.PlayerAddress.ToBase58()), context.ChainId);
        if (holder == null) return;

        var transIndex = new CAHolderTransactionIndex
        {
            Id = IdGenerateHelper.GetId(context.BlockHash, context.TransactionId),
            Timestamp = context.BlockTime.ToTimestamp().Seconds,
            FromAddress = eventValue.PlayerAddress.ToBase58(),
            TransactionFee = GetTransactionFee(context.ExtraProperties),
            TransferInfo = new TransferInfo
            {
                FromAddress = GetContractAddress(context.ChainId),
                ToAddress = eventValue.PlayerAddress.ToBase58(),
                Amount = (eventValue.Amount + eventValue.Award) / 100000000,
                FromChainId = context.ChainId,
                ToChainId = context.ChainId,
            },
        };
        ObjectMapper.Map(context, transIndex);
        transIndex.MethodName = GetMethodName(context.MethodName, context.Params);

        await CAHolderTransactionIndexRepository.AddOrUpdateAsync(transIndex);
        var index = await _bingoIndexRepository.GetFromBlockStateSetAsync(eventValue.PlayId.ToHex(), context.ChainId);
        if (index == null)
        {
            return;
        }
        // _objectMapper.Map<LogEventContext, CAHolderIndex>(context, caHolderIndex);
        index.BingoBlockHeight = context.BlockHeight;
        index.BingoId = context.TransactionId;
        index.BingoTime = context.BlockTime.ToTimestamp().Seconds;
        var feeMap = GetTransactionFee(context.ExtraProperties);
        List<TransactionFee> feeList;
        if (!feeMap.IsNullOrEmpty())
        {
            feeList = feeMap.Select(pair => new TransactionFee
            {
                Symbol = pair.Key,
                Amount = pair.Value
            }).ToList();
        }
        else
        {
            feeList = new List<TransactionFee>
            {
                new ()
                {
                    Symbol = null,
                    Amount = 0
                }
            };
        }
        index.BingoTransactionFee = feeList;
        index.IsComplete = true;
        index.Dices = eventValue.Dices.Dices.ToList();
        index.Award = eventValue.Award;
        index.BingoBlockHash = context.BlockHash;
        ObjectMapper.Map<LogEventContext, BingoGameIndex>(context, index);
        await _bingoIndexRepository.AddOrUpdateAsync(index);
        
        //update bingoStaticsIndex
        var staticsId= IdGenerateHelper.GetId(context.ChainId, eventValue.PlayerAddress.ToBase58());
        var bingoStaticsIndex = await _bingoStaticsIndexRepository.GetFromBlockStateSetAsync(staticsId, context.ChainId);
        if (bingoStaticsIndex == null)
        {
            bingoStaticsIndex = new BingoGameStaticsIndex
            {
                Id = staticsId,
                PlayerAddress = eventValue.PlayerAddress.ToBase58(),
                Amount = eventValue.Amount,
                Award = eventValue.Award,
                TotalWins = eventValue.Award > 0 ? 1 : 0,
                TotalPlays = 1
            };
        }
        else
        {
            bingoStaticsIndex.Amount += eventValue.Amount;
            bingoStaticsIndex.Award += eventValue.Award;
            bingoStaticsIndex.TotalPlays += 1;
            bingoStaticsIndex.TotalWins += eventValue.Award > 0 ? 1 : 0;
        }
        ObjectMapper.Map<LogEventContext, BingoGameStaticsIndex>(context, bingoStaticsIndex);
        await _bingoStaticsIndexRepository.AddOrUpdateAsync(bingoStaticsIndex); 
    }
}
