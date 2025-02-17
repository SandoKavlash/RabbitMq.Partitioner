using System;

namespace RabbitMq.Partition.Publisher.Abstractions;

public interface IPartitionedEventByGuid
{
    Guid PartitionKey { get; }
}