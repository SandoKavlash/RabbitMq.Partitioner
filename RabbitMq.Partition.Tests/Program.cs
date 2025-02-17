using MassTransit;
using Microsoft.Extensions.Hosting;
using RabbitMq.Partition.Publisher;

Host
    .CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddRabbitPartition((context, rabbitConfig, PartitionPublisherSettings) =>
        {
            rabbitConfig.Host("localhost", 5672, "PartitionTesting", (hostConfig) =>
            {
                hostConfig.Username("root");
                hostConfig.Password("root");
            });
        });
    })
    .Build()
    .Run();