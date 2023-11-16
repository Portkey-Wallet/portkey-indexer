using AElf;
using AElf.CSharp.Core;
using AElf.Types;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Portkey.Indexer.CA.Processors;

public abstract class CAHolderTransactionEventBase<TEvent> : AElfLogEventProcessorBase<TEvent, TransactionInfo>
    where TEvent : IEvent<TEvent>, new()
{
    protected CAHolderTransactionEventBase(ILogger<AElfLogEventProcessorBase<TEvent, TransactionInfo>> logger) : base(logger)
    {
    }
    
    protected Address ConvertVirtualAddressToContractAddress(
        Hash virtualAddress,
        Address contractAddress)
    {
        return Address.FromPublicKey(contractAddress.Value.Concat<byte>((IEnumerable<byte>) virtualAddress.Value.ToByteArray().ComputeHash()).ToArray<byte>());
    }

    protected Dictionary<string, long> GetTransactionFee(Dictionary<string, string> extraProperties)
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