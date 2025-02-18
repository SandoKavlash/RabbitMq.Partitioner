using System;
using MassTransit;

namespace RabbitMq.Partition.Publisher.Abstractions;

[ExcludeFromTopology]
public interface IPartitionedEventByGuid
{
    Guid PartitionKey { get; }
}