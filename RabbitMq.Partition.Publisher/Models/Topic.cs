namespace RabbitMq.Partition.Publisher.Models;

public class Topic
{
    public required string TopicName { get; set; }

    public required int PartitionsCount { get; set; }
}