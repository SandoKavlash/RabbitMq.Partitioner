namespace RabbitMq.Partition.Publisher.Abstractions;

public interface IPartitionedEventByString
{
    string PartitionKey { get; }
}