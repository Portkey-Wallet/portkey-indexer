using AElf;
using AElf.Types;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Client.Providers;
using AElfIndexer.Grains.State.Client;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Nito.AsyncEx;
using Orleans;
using Portkey.Contracts.CA;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.Handlers;

public class CAHolderTransactionHandler : TransactionDataHandler
{
    private readonly ContractInfoOptions _contractInfoOptions;
    private readonly CAHolderTransactionInfoOptions _caHolderTransactionInfoOptions;
    private readonly IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> _caHolderIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo> _caHolderTransactionIndexRepository;

    public CAHolderTransactionHandler(IClusterClient clusterClient, IObjectMapper objectMapper,
        IAElfIndexerClientInfoProvider aelfIndexerClientInfoProvider, IDAppDataProvider dAppDataProvider,
        IBlockStateSetProvider<TransactionInfo> blockStateSetProvider, IDAppDataIndexManagerProvider dAppDataIndexManagerProvider,
        IEnumerable<IAElfLogEventProcessor<TransactionInfo>> processors, ILogger<TransactionDataHandler> logger,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions, IOptionsSnapshot<CAHolderTransactionInfoOptions> caHolderTransactionInfoOptions,
        IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> caHolderIndexRepository,
        IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo> caHolderTransactionIndexRepository)
        : base(clusterClient, objectMapper, aelfIndexerClientInfoProvider, dAppDataProvider, blockStateSetProvider, dAppDataIndexManagerProvider, processors, logger)
    {
        _contractInfoOptions = contractInfoOptions.Value;
        _caHolderTransactionInfoOptions = caHolderTransactionInfoOptions.Value;
        _caHolderIndexRepository = caHolderIndexRepository;
        _caHolderTransactionIndexRepository = caHolderTransactionIndexRepository;
    }

    public async Task ProcessTransactionListAsync(List<TransactionInfo> transactions)
    {
        await ProcessTransactionsAsync(transactions);
    }

    protected async override Task ProcessTransactionsAsync(List<TransactionInfo> transactions)
    {
        var tasks = transactions.Select(ProcessTransactionsAsync).ToList();
        await tasks.WhenAll();
    }

    private async Task ProcessTransactionsAsync(TransactionInfo transactionInfo)
    {
        var transactionInfoOption = _contractInfoOptions.CATransactionInfos.FirstOrDefault(t => t.ChainId == transactionInfo.ChainId &&
            t.ContractAddress == transactionInfo.To && t.MethodName == transactionInfo.MethodName);
        if (transactionInfoOption == null)
        {
            return;
        }

        if (transactionInfo.MethodName == "ManagerForwardCall")
        {
            var managerForwardCallInput = ManagerForwardCallInput.Parser.ParseFrom(ByteString.FromBase64(transactionInfo.Params));
            if (transactionInfoOption.BlackMethodNames.Contains(managerForwardCallInput.MethodName))
            {
                return;
            }

            var caAddress = ConvertVirtualAddressToContractAddress(managerForwardCallInput.CaHash,
                Address.FromBase58(transactionInfo.To)).ToBase58();
            var holder = await _caHolderIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(transactionInfo.ChainId,
                caAddress), transactionInfo.ChainId);
            if (holder == null) return;

            var transIndex = new CAHolderTransactionIndex
            {
                Id = IdGenerateHelper.GetId(transactionInfo.BlockHash, transactionInfo.TransactionId),
                Timestamp = transactionInfo.BlockTime.ToTimestamp().Seconds,
                FromAddress = caAddress,
                TransactionFee = GetTransactionFee(transactionInfo.ExtraProperties),
            };
            ObjectMapper.Map(transactionInfo, transIndex);
            transIndex.MethodName = managerForwardCallInput.MethodName;
            await _caHolderTransactionIndexRepository.AddOrUpdateAsync(transIndex);
        }
    }

    private Address ConvertVirtualAddressToContractAddress(
        Hash virtualAddress,
        Address contractAddress)
    {
        return Address.FromPublicKey(contractAddress.Value.Concat<byte>((IEnumerable<byte>) virtualAddress.Value.ToByteArray().ComputeHash()).ToArray<byte>());
    }
    
    private Dictionary<string, long> GetTransactionFee(Dictionary<string, string> extraProperties)
    {
        var feeMap = new Dictionary<string, long>();
        if (extraProperties.TryGetValue("TransactionFee", out var transactionFee))
        {
            feeMap = JsonConvert.DeserializeObject<Dictionary<string, long>>(transactionFee) ??
                     new Dictionary<string, long>();
        }

        if (extraProperties.TryGetValue("ResourceFee", out var resourceFee))
        {
            var resourceFeeMap = JsonConvert.DeserializeObject<Dictionary<string, long>>(resourceFee) ??
                                 new Dictionary<string, long>();
            foreach (var (symbol, fee) in resourceFeeMap)
            {
                if (feeMap.TryGetValue(symbol, out _))
                {
                    feeMap[symbol] += fee;
                }
                else
                {
                    feeMap[symbol] = fee;
                }
            }
        }
        return feeMap;
    }
}