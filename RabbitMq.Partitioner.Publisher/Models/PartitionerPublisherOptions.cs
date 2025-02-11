using Microsoft.Extensions.DependencyInjection;
using RabbitMq.Partitioner.Publisher.Abstractions;
using RabbitMq.Partitioner.Publisher.Implementations;

namespace RabbitMq.Partitioner.Publisher.Models;

public class PartitionerPublisherOptions
{
    private readonly IServiceCollection _services;

    internal PartitionerPublisherOptions(IServiceCollection services)
    {
        _services = services;
    }

    private PartitionerPublisherOptions AddPartitionerPublisher<T>(PartitioningStrategy<T> messagePartitioningStrategy)
        where T : class
    {
        _services.AddSingleton<PartitioningStrategy<T>>(messagePartitioningStrategy);
        _services.AddScoped<IPartitionerPublisher<T>, PartitionerPublisher<T>>();
        PartitionerPublisher<T>._partitionBaseUri = $"queue:{messagePartitioningStrategy.TopicName}";
        return this;
    }
}