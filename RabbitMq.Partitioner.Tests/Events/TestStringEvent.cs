using RabbitMq.Partitioner.Abstractions;

namespace RabbitMq.Partitioner.Tests.Events;

public class TestStringEvent : IStringPartitionKeyable
{
    public string UserId { get; set; }
    public string PartitionKey => UserId;
}