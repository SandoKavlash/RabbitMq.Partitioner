using MassTransit.Futures.Contracts;

namespace RabbitMq.Partition.Publisher;

public class PartitionPublisherSettings
{
    public string? ConnectionString { get; set; } = null;

    public PersistenceProvider PersistenceProvider { get; set; } = PersistenceProvider.None;
}