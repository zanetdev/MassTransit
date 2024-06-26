namespace MassTransit.Transports
{
    public interface IMessagePartitionKeyFormatter<in TMessage>
        where TMessage : class
    {
        string FormatPartitionKey(SendContext<TMessage> context);
    }
}
