using System;

namespace RabbitMq.Partitioner.Abstractions;

public interface IGuidPartitionKeyable
{
    Guid PartitionKey { get; }
}