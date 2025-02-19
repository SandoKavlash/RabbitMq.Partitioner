namespace RabbitMq.Partitioner.Models;

public class RabbitPartitionerConfiguration
{
    internal List<Topic> Topics { get; set; } = new List<Topic>();

    internal RabbitPartitionerConfiguration() { }

    public void AddTopic(Topic topic)
    {
        Topics.Add(topic);
    }
}