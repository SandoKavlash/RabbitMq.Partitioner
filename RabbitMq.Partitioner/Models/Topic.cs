using EasyNetQ.Topology;
using RabbitMq.Partitioner.Exceptions.Base;

namespace RabbitMq.Partitioner.Models;

public class Topic
{
    internal string Name { get; set; }

    internal int PartitionsCount { get; set; }

    internal Exchange? CreatedExchange { get; set; } = null;

    public Topic WithName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new RabbitPartitionerException("Topic name cannot be null or empty");
        }
        Name = name;
        return this;
    }

    public Topic WithPartitionsCount(int count)
    {
        if (count <= 0)
        {
            throw new RabbitPartitionerException($"partition count cannot be less or equal to 0. your specified count: {count}");
        }

        PartitionsCount = count;
        return this;
    }
}