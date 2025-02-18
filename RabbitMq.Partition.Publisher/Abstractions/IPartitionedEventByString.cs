using MassTransit;

namespace RabbitMq.Partition.Publisher.Abstractions;

[ExcludeFromTopology]
public interface IPartitionedEventByString
{
    string PartitionKey { get; }
}