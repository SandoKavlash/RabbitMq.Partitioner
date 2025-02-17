using MassTransit;
using Microsoft.Extensions.Hosting;
using RabbitMq.Partition.Publisher;
using RabbitMq.Partition.Publisher.Models;

Host
    .CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddRabbitPartition((context, rabbitConfig, partitionPublisherSettings) =>
        {
            rabbitConfig.Host("localhost", 5672, "PartitionTesting", (hostConfig) =>
            {
                hostConfig.Username("root");
                hostConfig.Password("root");
            });
            partitionPublisherSettings.Topics.Add(new Topic()
            {
                TopicName = "Test",
                PartitionsCount = 15
            });
        });
    })
    .Build()
    .Run();