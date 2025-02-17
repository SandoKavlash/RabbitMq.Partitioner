using System;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using RabbitMq.Partition.Publisher.Abstractions;
using RabbitMq.Partition.Publisher.Enums;
using RabbitMq.Partition.Publisher.Implementations;
using RabbitMq.Partition.Publisher.Jobs;
using RabbitMq.Partition.Publisher.Models;

namespace RabbitMq.Partition.Publisher;

public static class Extensions
{
    public static IServiceCollection AddRabbitPartition(this IServiceCollection services,
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator, PartitionPublisherSettings> configure)
    {
        PartitionPublisherSettings settings = new PartitionPublisherSettings();

        ConfigureMassTransit(services, configure, settings);
        RegisterPartitionPublisher(services, settings);
        SaveTopicsMetadata(settings);
        
        services.AddSingleton<PartitionPublisherSettings>(settings);
        return services;
    }

    private static void SaveTopicsMetadata(PartitionPublisherSettings settings)
    {
        foreach (var topic in settings.Topics)
        {
            Constants.TopicsAndPartitionsMapping.Add(topic.TopicName,topic.PartitionsCount);
        }
    }

    private static void ConfigureMassTransit(IServiceCollection services, Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator, PartitionPublisherSettings> configure,
        PartitionPublisherSettings settings)
    {
        services.AddMassTransit<IPartitionBus>(massTransitConfig =>
        {
            massTransitConfig.UsingRabbitMq((context, rabbitConfig) =>
            {
                configure(context, rabbitConfig, settings); // after this method call, settings instance will be populated.

                SetupTopics(settings, rabbitConfig);
                rabbitConfig.ConfigureEndpoints(context);
            });
        });
    }

    private static void SetupTopics(PartitionPublisherSettings settings, IRabbitMqBusFactoryConfigurator rabbitConfig)
    {
        foreach (Topic topic in settings.Topics)
        {
            SetupTopic(rabbitConfig, topic);
        }
    }

    private static void SetupTopic(IRabbitMqBusFactoryConfigurator rabbitConfig, Topic topic)
    {
        for (int i = 0; i < topic.PartitionsCount; i++)
        {
            rabbitConfig.ReceiveEndpoint($"{topic.TopicName}-{i}", endpointConfig =>
            {
                endpointConfig.Durable = true;
                endpointConfig.Exclusive = false;
                endpointConfig.AutoDelete = false;
                endpointConfig.SingleActiveConsumer = true;
            });
        }
    }

    private static void RegisterPartitionPublisher(IServiceCollection services, PartitionPublisherSettings settings)
    {
        if (settings.ConnectionString == null) // If connection string is null, it means that user don't need outbox
        {
            services.AddScoped<IPartitionPublisher, PartitionPublisherStraightToTheRabbit>();
        }
        else // If connection string is not null, it means that user wants to configure outbox pattern for publishing
        {
            services.AddScoped(typeof(IPartitionPublisher),settings.GetPartitionPublisherType());
            services.AddHostedService<OutboxPublisherJob>();
        }
    }

    private static Type GetPartitionPublisherType(this PartitionPublisherSettings settings)
    {
        return settings.PersistenceProvider switch
        {
            PersistenceProvider.None => throw new ArgumentException(
                "ConnectionString is specified but provider not. you must specify DB provider"),
            PersistenceProvider.PostgreSql => typeof(PartitionPublisherWithOutboxPostgres),
            _ => throw new ArgumentException(
                "Implementation, for specified provider, doesn't exist")
        };
    }
}