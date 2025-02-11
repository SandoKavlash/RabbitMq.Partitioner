using System;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using RabbitMq.Partitioner.Publisher.MassTransitHelpers;
using RabbitMq.Partitioner.Publisher.Models;

namespace RabbitMq.Partitioner.Publisher;

public static class Extensions
{
    public static IServiceCollection AddPartitionerPublisher(this IServiceCollection services,
        Action<PartitionerPublisherOptions> optionsAction, Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator> configureRabbitMq)
    {
        PartitionerPublisherOptions options = new PartitionerPublisherOptions(services);

        services.AddMassTransit<IMassTransitPartitionerPublisher>(massTransitConfig =>
        {
            massTransitConfig.UsingRabbitMq(configureRabbitMq);
        });

        optionsAction(options);
        return services;
    }
}