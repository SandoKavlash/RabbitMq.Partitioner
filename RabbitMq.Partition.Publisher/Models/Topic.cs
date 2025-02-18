using System;
using MassTransit;
using RabbitMQ.Client;

namespace RabbitMq.Partition.Publisher.Models;

public abstract class Topic
{
    public required string TopicName { get; set; }

    public required int PartitionsCount { get; set; }

    internal Type MessageType { get; private set; }

    internal abstract Action<IRabbitMqBusFactoryConfigurator, Topic> SetUpTopicDelegate { get; }

    internal Topic(Type messageType)
    {
        MessageType = messageType;
    }
}
public class Topic<T> : Topic
    where T : class
{
    public Topic() : base(typeof(T)) { }
    internal override Action<IRabbitMqBusFactoryConfigurator, Topic> SetUpTopicDelegate => (rabbitConfig, topic) =>
    {
        rabbitConfig.Message<T>(x => x.SetEntityName(topic.TopicName));
        rabbitConfig.Publish(topic.MessageType, publishConfig =>
        {
            publishConfig.Durable = true;
            publishConfig.ExchangeType = ExchangeType.Direct;
            for (int i = 0; i < topic.PartitionsCount; i++)
            {
                string partition = $"{topic.TopicName}-{i}";
                publishConfig.BindQueue(topic.MessageType.FullName!, partition, bindConfig =>
                {
                    bindConfig.RoutingKey = partition; // Set the routing key if needed
                    bindConfig.Durable = true;
                    bindConfig.Exclusive = false;
                    bindConfig.AutoDelete = false;
                    bindConfig.SingleActiveConsumer = true;
                });
            }
        });
    };
}