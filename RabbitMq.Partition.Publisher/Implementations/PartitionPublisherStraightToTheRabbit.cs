using System.Threading;
using System.Threading.Tasks;
using RabbitMq.Partition.Publisher.Abstractions;

namespace RabbitMq.Partition.Publisher.Implementations;

internal class PartitionPublisherStraightToTheRabbit : IPartitionPublisher
{
    public Task PublishAsync(IPartitionedEventByGuid data, string topic, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    public Task PublishAsync(IPartitionedEventByString data, string topic, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }
}