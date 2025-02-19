namespace RabbitMq.Partitioner.Abstractions;

public interface IStringPartitionKeyable
{
    string PartitionKey { get; }
}