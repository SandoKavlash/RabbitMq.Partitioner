using System.Threading;
using System.Threading.Tasks;

namespace RabbitMq.Partition.Publisher.Abstractions;

public interface IPartitionPublisher
{
    Task PublishGuidEventAsync<TMessage>(TMessage data, CancellationToken cancellationToken = default)
        where TMessage : class, IPartitionedEventByGuid;

    Task PublishStringEventAsync<TMessage>(TMessage data, CancellationToken cancellationToken = default)
        where TMessage : class, IPartitionedEventByString;
}