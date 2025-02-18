using System.Threading;
using System.Threading.Tasks;
using RabbitMq.Partition.Publisher.Abstractions;

namespace RabbitMq.Partition.Publisher.Implementations;

internal class PartitionPublisherWithOutboxPostgres : IPartitionPublisher
{
    public Task PublishGuidEventAsync<TMessage>(TMessage data, CancellationToken cancellationToken = default) where TMessage : class, IPartitionedEventByGuid
    {
        throw new System.NotImplementedException();
    }

    public Task PublishStringEventAsync<TMessage>(TMessage data, CancellationToken cancellationToken = default) where TMessage : class, IPartitionedEventByString
    {
        throw new System.NotImplementedException();
    }
}