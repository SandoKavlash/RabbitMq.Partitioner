using System;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using RabbitMQ.Client;
using RabbitMq.Partition.Publisher.Abstractions;
using RabbitMq.Partition.Publisher.Enums;
using RabbitMq.Partition.Publisher.Implementations;
using RabbitMq.Partition.Publisher.Jobs;
using RabbitMq.Partition.Publisher.Models;

namespace RabbitMq.Partition.Publisher;

public static class Extensions
{
    public static IServiceCollection AddRabbitPartitioner(
        this IServiceCollection services,
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator> configureRabbit, 
        Action<PartitionPublisherSettings> configurePartitionPublisher)
    {
        PartitionPublisherSettings settings = new PartitionPublisherSettings();
        configurePartitionPublisher(settings);

        ConfigureMassTransit(services, configureRabbit, settings);
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

    private static void ConfigureMassTransit(
        IServiceCollection services, 
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator> configureRabbit,
        PartitionPublisherSettings settings)
    {
        services.AddMassTransit<IPartitionBus>(massTransitConfig =>
        {
            massTransitConfig.UsingRabbitMq((context, rabbitConfig) =>
            {
                configureRabbit(context, rabbitConfig); // after this method call, settings instance will be populated.

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
        rabbitConfig.Message(x => x.SetEntityName("custom-exchange-name"));// TODO: correct this one
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