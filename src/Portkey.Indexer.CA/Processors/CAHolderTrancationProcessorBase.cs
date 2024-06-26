using AElf.CSharp.Core;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Portkey.Indexer.CA.Provider;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Processors;

public abstract class CAHolderTransactionProcessorBase<TEvent> : AElfLogEventProcessorBase<TEvent, TransactionInfo>
    where TEvent : IEvent<TEvent>, new()
{
    protected readonly IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> CAHolderIndexRepository;

    protected readonly IAElfIndexerClientEntityRepository<CAHolderManagerIndex, TransactionInfo>
        CAHolderManagerIndexRepository;

    protected readonly IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>
        CAHolderTransactionIndexRepository;

    protected readonly IAElfIndexerClientEntityRepository<TokenInfoIndex, TransactionInfo> TokenInfoIndexRepository;
    protected readonly IAElfIndexerClientEntityRepository<NFTInfoIndex, TransactionInfo> NFTInfoIndexRepository;

    protected readonly IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo>
        CAHolderTransactionAddressIndexRepository;

    private readonly IAElfDataProvider _aelfDataProvider;

    protected readonly ContractInfoOptions ContractInfoOptions;
    protected readonly CAHolderTransactionInfoOptions CAHolderTransactionInfoOptions;
    protected readonly IObjectMapper ObjectMapper;
    private const string FullAddressPrefix = "ELF";
    private const char FullAddressSeparator = '_';

    protected CAHolderTransactionProcessorBase(ILogger<CAHolderTransactionProcessorBase<TEvent>> logger,
        IAElfIndexerClientEntityRepository<CAHolderIndex, TransactionInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderManagerIndex, TransactionInfo> caHolderManagerIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo>
            caHolderTransactionIndexRepository,
        IAElfIndexerClientEntityRepository<TokenInfoIndex, TransactionInfo> tokenInfoIndexRepository,
        IAElfIndexerClientEntityRepository<NFTInfoIndex, TransactionInfo> nftInfoIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, TransactionInfo>
            caHolderTransactionAddressIndexRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions,
        IObjectMapper objectMapper, IAElfDataProvider aelfDataProvider = null) : base(logger)
    {
        CAHolderIndexRepository = caHolderIndexRepository;
        CAHolderManagerIndexRepository = caHolderManagerIndexRepository;
        CAHolderTransactionIndexRepository = caHolderTransactionIndexRepository;
        NFTInfoIndexRepository = nftInfoIndexRepository;
        TokenInfoIndexRepository = tokenInfoIndexRepository;
        ContractInfoOptions = contractInfoOptions.Value;
        CAHolderTransactionInfoOptions = caHolderTransactionInfoOptions?.Value;
        ObjectMapper = objectMapper;
        CAHolderTransactionAddressIndexRepository = caHolderTransactionAddressIndexRepository;
        _aelfDataProvider = aelfDataProvider;
    }

    protected bool IsValidTransaction(string chainId, string to, string methodName, string parameter)
    {
        if (!CAHolderTransactionInfoOptions.CAHolderTransactionInfos.Where(t => t.ChainId == chainId).Any(t =>
                t.ContractAddress == to && t.MethodName == methodName &&
                t.EventNames.Contains(GetEventName()))) return false;
        if (methodName == "ManagerForwardCall" &&
            !IsValidManagerForwardCallTransaction(chainId, to, methodName, parameter)) return false;
        return true;
    }

    protected string GetToContractAddress(string chainId, string to, string methodName, string parameter)
    {
        if (to == ContractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).CAContractAddress &&
            methodName == "ManagerForwardCall")
        {
            var managerForwardCallInput = ManagerForwardCallInput.Parser.ParseFrom(ByteString.FromBase64(parameter));
            return managerForwardCallInput.ContractAddress.ToBase58();
        }

        return to;
    }

    protected bool IsMultiTransaction(string chainId, string to, string methodName)
    {
        var caHolderTransactionInfo = CAHolderTransactionInfoOptions.CAHolderTransactionInfos.FirstOrDefault(t =>
            t.ChainId == chainId &&
            t.ContractAddress == to && t.MethodName == methodName &&
            t.EventNames.Contains(GetEventName()));
        return caHolderTransactionInfo?.MultiTransaction ?? false;
    }

    private bool IsValidManagerForwardCallTransaction(string chainId, string to, string methodName, string parameter)
    {
        if (methodName != "ManagerForwardCall") return false;
        if (to != ContractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).CAContractAddress && to !=
            ContractInfoOptions.ContractInfos.First(c => c.ChainId == chainId).AnotherCAContractAddress) return false;
        var managerForwardCallInput = ManagerForwardCallInput.Parser.ParseFrom(ByteString.FromBase64(parameter));
        return IsValidTransaction(chainId, managerForwardCallInput.ContractAddress.ToBase58(),
            managerForwardCallInput.MethodName, managerForwardCallInput.Args.ToBase64());
    }

    protected string GetMethodName(string methodName, string parameter)
    {
        if (methodName == "ManagerTransfer") return "Transfer";
        if (methodName != "ManagerForwardCall") return methodName;
        var managerForwardCallInput = ManagerForwardCallInput.Parser.ParseFrom(ByteString.FromBase64(parameter));
        return GetMethodName(managerForwardCallInput.MethodName, managerForwardCallInput.Args.ToBase64());
    }

    protected Dictionary<string, long> GetTransactionFee(Dictionary<string, string> extraProperties)
    {
        return new Dictionary<string, long>();
    }

    protected async Task AddCAHolderTransactionAddressAsync(string caAddress, string address, string addressChainId,
        LogEventContext context)
    {
        var id = IdGenerateHelper.GetId(context.ChainId, caAddress, address, addressChainId);
        var caHolderTransactionAddressIndex =
            await CAHolderTransactionAddressIndexRepository.GetFromBlockStateSetAsync(id, context.ChainId);
        if (caHolderTransactionAddressIndex == null)
        {
            caHolderTransactionAddressIndex = new CAHolderTransactionAddressIndex
            {
                Id = id,
                CAAddress = caAddress,
                Address = address,
                AddressChainId = addressChainId
            };
        }

        var transactionTime = context.BlockTime.ToTimestamp().Seconds;
        if (caHolderTransactionAddressIndex.TransactionTime >= transactionTime) return;
        caHolderTransactionAddressIndex.TransactionTime = transactionTime;
        ObjectMapper.Map(context, caHolderTransactionAddressIndex);
        await CAHolderTransactionAddressIndexRepository.AddOrUpdateAsync(caHolderTransactionAddressIndex);
    }

    protected async Task<string> ProcessCAHolderTransactionAsync(LogEventContext context, string caAddress)
    {
        if (!IsValidTransaction(context.ChainId, context.To, context.MethodName, context.Params)) return null;
        var holder = await CAHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId,
            caAddress), context.ChainId);
        if (holder == null) return null;

        var id = IdGenerateHelper.GetId(context.BlockHash, context.TransactionId);
        var transIndex = await CAHolderTransactionIndexRepository.GetFromBlockStateSetAsync(id, context.ChainId);
        var transactionFee = GetTransactionFee(context.ExtraProperties);
        if (transIndex != null)
        {
            transactionFee = transIndex.TransactionFee.IsNullOrEmpty() ? transactionFee : transIndex.TransactionFee;
        }

        var index = new CAHolderTransactionIndex
        {
            Id = id,
            Timestamp = context.BlockTime.ToTimestamp().Seconds,
            FromAddress = caAddress,
            TransactionFee = transactionFee
        };
        ObjectMapper.Map(context, index);
        index.MethodName = GetMethodName(context.MethodName, context.Params);

        await CAHolderTransactionIndexRepository.AddOrUpdateAsync(index);

        return holder.CAAddress;
    }

    protected long GetFeeAmount(Dictionary<string, string> extraProperties)
    {
        var feeMap = GetTransactionFee(extraProperties);
        if (feeMap.TryGetValue("ELF", out var value))
        {
            return value;
        }

        return 0;
    }

    protected async Task<NFTInfoIndex> GetNftInfoIndexFromStateOrChainAsync(string symbol, LogEventContext context)
    {
        if (TokenHelper.GetTokenType(symbol) != TokenType.NFTItem)
        {
            return null;
        }

        var nftInfoIndex =
            await NFTInfoIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(context.ChainId, symbol),
                context.ChainId);
        if (nftInfoIndex != null || _aelfDataProvider == null)
        {
            return nftInfoIndex;
        }

        var nftInfoId = IdGenerateHelper.GetId(context.ChainId, symbol);
        nftInfoIndex = new NFTInfoIndex()
        {
            Id = nftInfoId,
            Symbol = symbol,
            Type = TokenType.NFTItem,
            TokenContractAddress = GetContractAddress(context.ChainId)
        };
        ObjectMapper.Map(context, nftInfoIndex);
        var nftInfo = await _aelfDataProvider.GetTokenInfoAsync(nftInfoIndex.ChainId, nftInfoIndex.Symbol);
        if (nftInfo.Symbol == nftInfoIndex.Symbol)
        {
            ObjectMapper.Map(nftInfo, nftInfoIndex);
            if (nftInfo.ExternalInfo is { Count: > 0 })
            {
                nftInfoIndex.ExternalInfoDictionary = nftInfo.ExternalInfo
                    .Where(t => !t.Key.IsNullOrWhiteSpace())
                    .ToDictionary(item => item.Key, item => item.Value);


                if (nftInfo.ExternalInfo.TryGetValue("__nft_image_url", out var image))
                {
                    nftInfoIndex.ImageUrl = image;
                }
                else if (nftInfo.ExternalInfo.TryGetValue("inscription_image", out var inscriptionImage))
                {
                    nftInfoIndex.ImageUrl = inscriptionImage;
                }
                else if (nftInfo.ExternalInfo.TryGetValue("__nft_image_uri", out var inscriptionImageUrl))
                {
                    nftInfoIndex.ImageUrl = inscriptionImageUrl;
                }
                else if (nftInfo.ExternalInfo.TryGetValue("__inscription_image", out var imageUrl))
                {
                    nftInfoIndex.ImageUrl = imageUrl;
                }
            }

            nftInfoIndex.ExternalInfoDictionary ??= new Dictionary<string, string>();
        }

        return nftInfoIndex;
    }

    protected async Task<TokenInfoIndex> GetTokenInfoIndexFromStateOrChainAsync(string symbol, LogEventContext context)
    {
        if (TokenHelper.GetTokenType(symbol) != TokenType.Token)
        {
            return null;
        }

        var tokenInfoIndex = await TokenInfoIndexRepository.GetFromBlockStateSetAsync(
            IdGenerateHelper.GetId(context.ChainId, symbol),
            context.ChainId);
        if (tokenInfoIndex != null || _aelfDataProvider == null)
        {
            return tokenInfoIndex;
        }

        tokenInfoIndex = new TokenInfoIndex
        {
            Id = IdGenerateHelper.GetId(context.ChainId, symbol),
            TokenContractAddress = GetContractAddress(context.ChainId),
            Type = TokenType.Token,
            Symbol = symbol
        };
        ObjectMapper.Map(context, tokenInfoIndex);
        var tokenInfo = await _aelfDataProvider.GetTokenInfoAsync(tokenInfoIndex.ChainId, tokenInfoIndex.Symbol);
        if (tokenInfo.Symbol == tokenInfoIndex.Symbol)
        {
            ObjectMapper.Map(tokenInfo, tokenInfoIndex);
            if (tokenInfo.ExternalInfo is { Count: > 0 })
            {
                tokenInfoIndex.ExternalInfoDictionary = tokenInfo.ExternalInfo
                    .Where(t => !t.Key.IsNullOrWhiteSpace())
                    .ToDictionary(item => item.Key, item => item.Value);
            }

            tokenInfoIndex.ExternalInfoDictionary ??= new Dictionary<string, string>();
        }

        return tokenInfoIndex;
    }

    protected virtual async Task HandlerTransactionIndexAsync(TEvent eventValue, LogEventContext context)
    {
    }
}