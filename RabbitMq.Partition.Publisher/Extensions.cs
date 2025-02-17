using System;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using RabbitMq.Partition.Publisher.Implementations;
using RabbitMq.Partition.Publisher.Jobs;

namespace RabbitMq.Partition.Publisher;

public static class Extensions
{
    public static IServiceCollection AddRabbitPartition(this IServiceCollection services,
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator, PartitionPublisherSettings> configure)
    {
        PartitionPublisherSettings settings = new PartitionPublisherSettings();
        ConfigureMassTransit(services, configure, settings);
        services.AddSingleton<PartitionPublisherSettings>(settings);
        RegisterPartitionPublisher(services, settings);

        return services;
    }

    private static void ConfigureMassTransit(IServiceCollection services, Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator, PartitionPublisherSettings> configure,
        PartitionPublisherSettings settings)
    {
        services.AddMassTransit<IPartitionBus>(massTransitConfig =>
        {
            massTransitConfig.UsingRabbitMq((context, rabbitConfig) =>
            {
                configure(context, rabbitConfig, settings);
                rabbitConfig.ConfigureEndpoints(context);
            });
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