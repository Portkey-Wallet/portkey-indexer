namespace Portkey.Indexer.CA.Entities;

public class TransactionInfoIndex
{
    public string TransactionId { get; set; }

    public long TransactionFee { get; set; }

    public DateTime TriggerTime { get; set; }
}