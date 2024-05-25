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
        return new Dictionary<string, long>();
    }

}