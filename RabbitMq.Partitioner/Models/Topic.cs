namespace RabbitMq.Partitioner.Models;

public class Topic
{
    public required string Name { get; init; }

    public required int PartitionsCount { get; init; }
}