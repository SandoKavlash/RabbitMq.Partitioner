using System.Threading;
using System.Threading.Tasks;
using RabbitMq.Partition.Publisher.Abstractions;

namespace RabbitMq.Partition.Publisher.Implementations;

internal class PartitionPublisherWithOutboxPostgres : IPartitionPublisher
{
    public Task PublishAsync(IPartitionedEventByGuid data, string topic, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    public Task PublishAsync<TMessage>(TMessage data, string topic, CancellationToken cancellationToken = default) where TMessage : IPartitionedEventByString
    {
        throw new System.NotImplementedException();
    }
}