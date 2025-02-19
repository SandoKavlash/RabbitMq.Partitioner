using System;
using System.Collections.Generic;
using RabbitMq.Partitioner.Exceptions.Base;

namespace RabbitMq.Partitioner.Models;

public class RabbitPartitionerConfiguration
{
    internal List<Topic> Topics { get; set; } = new List<Topic>();

    internal RabbitPartitionerConfiguration() { }

    public RabbitPartitionerConfiguration ConfigureTopic(Action<Topic> topicConfig)
    {
        Topic topic = new Topic();

        topicConfig(topic);
        EnsureTopicIsValid(topic);

        Topics.Add(topic);

        return this;
    }

    private static void EnsureTopicIsValid(Topic topic)
    {
        if (string.IsNullOrEmpty(topic.Name))
        {
            throw new RabbitPartitionerException(
                $"{nameof(Topic)} configuration is not correct. Topic name cannot be null or empty");
        }

        if (topic.PartitionsCount <= 0)
        {
            throw new RabbitPartitionerException($"partition count cannot be less or equal to 0. your specified count: {topic.PartitionsCount}");
        }
    }
}