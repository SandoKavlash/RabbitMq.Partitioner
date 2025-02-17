using System.Threading;
using System.Threading.Tasks;

namespace RabbitMq.Partition.Publisher.Abstractions;

public interface IPartitionPublisher
{
    Task PublishAsync(IPartitionedEventByGuid data, string topic, CancellationToken cancellationToken = default);

    Task PublishAsync(IPartitionedEventByString data, string topic, CancellationToken cancellationToken = default);
}