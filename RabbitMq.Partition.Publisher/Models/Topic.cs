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
public class Topic<TEvent> : Topic
    where TEvent : class
{
    public Topic() : base(typeof(TEvent)) { }
    internal override Action<IRabbitMqBusFactoryConfigurator, Topic> SetUpTopicDelegate => (rabbitConfig, topic) =>
    {
        rabbitConfig.DeployPublishTopology = true; // Deploy topology on startup

        rabbitConfig.Message<TEvent>(x => x.SetEntityName(topic.TopicName));

        rabbitConfig.Publish<TEvent>(publishConfig =>
        {
            publishConfig.Durable = true;
            publishConfig.ExchangeType = ExchangeType.Direct;
            for (int i = 0; i < topic.PartitionsCount; i++)
            {
                string partitionName = $"{topic.TopicName}-{i}";
    
                publishConfig.BindQueue(topic.TopicName, partitionName, bindConfig =>
                {
                    bindConfig.RoutingKey = partitionName; // Set the routing key if needed
                    bindConfig.Durable = true;
                    bindConfig.Exclusive = false;
                    bindConfig.AutoDelete = false;
                    bindConfig.SingleActiveConsumer = true;
                });
            }
        });
    };
}