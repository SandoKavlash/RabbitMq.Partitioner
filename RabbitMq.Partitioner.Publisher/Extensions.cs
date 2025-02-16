using Microsoft.Extensions.DependencyInjection;
using MassTransit;

namespace RabbitMq.Partitioner.Publisher;

public static class Extensions
{
    public static IServiceCollection AddRabbitPartitioner(this IServiceCollection services)
    {
        services.AddMassTransit<IPartitionerBus>(massTransitConfig =>
        {
            massTransitConfig.UsingRabbitMq((context, rabbitConfig) =>
            {
                rabbitConfig.Host("localhost", 5672, "PartitionerTesting", (hostConfig) =>
                {
                    hostConfig.Username("root");
                    hostConfig.Password("root");
                });
                rabbitConfig.ConfigureEndpoints(context);
            });
        });
        return services;
    }
}