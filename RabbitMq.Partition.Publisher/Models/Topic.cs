using System;
using MassTransit;
using MassTransit.Middleware.Outbox;
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

        rabbitConfig.Message<TEvent>(topologyConfig => topologyConfig.SetEntityName(topic.TopicName));
        rabbitConfig.Publish<TEvent>(publishConfig =>
        {
            // Exchange configs:
            publishConfig.Durable = true;
            publishConfig.AutoDelete = false;
            publishConfig.ExchangeType = ExchangeType.Direct;
            //=================
        });

        for (int i = 0; i < topic.PartitionsCount; i++)
        {
            string partitionName = $"{topic.TopicName}-{i}";
            rabbitConfig.ReceiveEndpoint(partitionName, configureEndpoint =>
            {
                configureEndpoint.ConfigureConsumeTopology = false; // Prevents MassTransit from configuring a consumer

                // Queue configs:
                configureEndpoint.Durable = true;
                configureEndpoint.Exclusive = false;
                configureEndpoint.AutoDelete = false;
                configureEndpoint.SingleActiveConsumer = true;
                //===============
                configureEndpoint.Bind(topic.TopicName, bindingConfig =>
                {
                    // Exchange configs:
                    bindingConfig.RoutingKey = partitionName;
                    bindingConfig.Durable = true;
                    bindingConfig.AutoDelete = false;
                    bindingConfig.ExchangeType = ExchangeType.Direct;
                    //==================
                });
            });
        }
    };
}