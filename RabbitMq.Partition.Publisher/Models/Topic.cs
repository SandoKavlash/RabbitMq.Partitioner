using System;
using MassTransit;
using MassTransit.Middleware.Outbox;
using RabbitMQ.Client;

namespace RabbitMq.Partition.Publisher.Models;

public abstract class Topic
{
    public abstract required string TopicName { get; set; }

    public required int PartitionsCount { get; set; }

    internal Type MessageType { get; private set; }

    internal abstract Action<IRabbitMqBusFactoryConfigurator, Topic, Action<IRabbitMqBusFactoryConfigurator>> SetUpTopicDelegate { get; }

    internal Topic(Type messageType)
    {
        MessageType = messageType;
    }
}
public class Topic<TEvent> : Topic
    where TEvent : class
{
    internal static string? TopicNameCache = null;

    public override required string? TopicName
    {
        get => Topic<TEvent>.TopicNameCache;
        set => Topic<TEvent>.TopicNameCache = value;
    }

    public Topic() : base(typeof(TEvent)) { }

    internal override Action<IRabbitMqBusFactoryConfigurator, Topic, Action<IRabbitMqBusFactoryConfigurator>> SetUpTopicDelegate => 
        (rabbitConfig, topic, rabbitConfigurator) =>
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
        var queueSetupBus = Bus.Factory.CreateUsingRabbitMq(queueSetupRabbitConfig =>
        {
            rabbitConfigurator(queueSetupRabbitConfig);
            ConfigureQueues(topic, queueSetupRabbitConfig);
        });

        // Starts and stops immediately. it prevents consumers which are not necessary 
        queueSetupBus.Start();
        queueSetupBus.Stop();
    };

    private static void ConfigureQueues(Topic topic, IRabbitMqBusFactoryConfigurator queueSetupRabbitConfig)
    {
        for (int i = 0; i < topic.PartitionsCount; i++)
        {
            string partitionName = $"{topic.TopicName}-{i}";
            queueSetupRabbitConfig.ReceiveEndpoint(partitionName, configureEndpoint =>
            {
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
    }
}