using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMq.Partition.Publisher;
using RabbitMq.Partition.Publisher.Models;
using RabbitMq.Partition.Tests;

Host
    .CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddRabbitPartitioner((context, rabbitConfig) =>
        {
            rabbitConfig.Host("localhost", 5672, "PartitionTesting8", (hostConfig) =>
            {
                hostConfig.Username("root");
                hostConfig.Password("root");
            });
        }, partitionPublisherSettings =>
        {
            partitionPublisherSettings.Topics.Add(new Topic<TestEvent>()
            {
                TopicName = "Test",
                PartitionsCount = 15,
            });
        });
        services.AddHostedService<TestHostedService>();
    })
    .Build()
    .Run();