using System;
using System.Collections.Generic;
using EasyNetQ;
using EasyNetQ.Topology;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RabbitMq.Partitioner.Abstractions;
using RabbitMq.Partitioner.Implementations;
using RabbitMq.Partitioner.Models;

namespace RabbitMq.Partitioner;

public static class PartitionerExtensions
{
    public static IServiceCollection AddPartitioner(
        this IServiceCollection services, 
        Action<RabbitPartitionerConfiguration> configurator, 
        string rabbitConnectionString)
    {
        RabbitPartitionerConfiguration config = new RabbitPartitionerConfiguration();
        SetupConfiguration(configurator, config);

        services
            .AddEasyNetQ(rabbitConnectionString)
            .UseSystemTextJson();
        services.AddSingleton<RabbitPartitionerConfiguration>(config);
        services.AddSingleton<IPartitionPublisher, PartitionPublisherDirectlyInRabbit>();

        return services;
    }

    private static void SetupConfiguration(Action<RabbitPartitionerConfiguration> configurator, RabbitPartitionerConfiguration config)
    {
        configurator(config);
        foreach (Topic topic in config.Topics)
        {
            Constants.Topics.Add(topic.Name,topic);
        }
    }

    public static IApplicationBuilder UsePartitioner(this IApplicationBuilder builder)
    {
        IAdvancedBus advancedBus = builder.ApplicationServices.GetRequiredService<IBus>().Advanced;
        ConfigureTopology(advancedBus);
        return builder;
    }

    private static void ConfigureTopology(IAdvancedBus advancedBus)
    {
        foreach (KeyValuePair<string, Topic> topic in Constants.Topics)
        {
            ConfigureTopic(topic.Value, advancedBus);
        }
    }

    private static void ConfigureTopic(Topic topic, IAdvancedBus advancedBus)
    {
        Exchange topicExchange = advancedBus.ExchangeDeclare(topic.Name, config => config
            .AsDurable(true)
            .AsAutoDelete(false)
            .WithType(ExchangeType.Direct));
        topic.CreatedExchange = topicExchange;

        ConfigurePartitions(topic, topicExchange, advancedBus);
    }

    private static void ConfigurePartitions(Topic topic, Exchange topicExchange, IAdvancedBus advancedBus)
    {
        for (int i = 0; i < topic.PartitionsCount; i++)
        {
            ConfigurePartition(topic, topicExchange, advancedBus, i);
        }
    }

    private static void ConfigurePartition(Topic topic, Exchange topicExchange, IAdvancedBus advancedBus, int partitionId)
    {
        string partitionName = $"{topic.Name}-{partitionId}";
        Exchange partitionExchange = advancedBus.ExchangeDeclare(partitionName, config => config
                .AsDurable(true)
                .AsAutoDelete(false)
                .WithType(ExchangeType.Fanout));

        BindPartitionToTopic(
            topicExchange: topicExchange, 
            partitionExchange: partitionExchange, 
            routingKey: partitionName, 
            advancedBus: advancedBus);
    }

    private static void BindPartitionToTopic(Exchange topicExchange, Exchange partitionExchange, string routingKey, IAdvancedBus advancedBus)
    {
        // Binding 'topicExchange' to 'partitionExchange' with routing key 'routingKey'. [topicExchange ---routingKey---> partitionExchange]
        advancedBus.Bind(topicExchange, partitionExchange, routingKey);
    }
}