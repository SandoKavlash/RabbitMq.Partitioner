using System.Threading;
using System.Threading.Tasks;

namespace RabbitMq.Partitioner.Abstractions;

public interface IPartitionPublisher
{
    Task PublishAsync(IGuidPartitionKeyable @event, string topicName, CancellationToken cancellationToken = default);

    Task PublishAsync(IStringPartitionKeyable @event, string topicName, CancellationToken cancellationToken = default);
}