using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.DependencyInjection;
using RabbitMq.Partitioner.Publisher.Abstractions;
using RabbitMq.Partitioner.Publisher.MassTransitHelpers;
using RabbitMq.Partitioner.Publisher.Models;

namespace RabbitMq.Partitioner.Publisher.Implementations;

public class PartitionerPublisher<TMessage> : IPartitionerPublisher<TMessage>
    where TMessage : class
{
    private readonly PartitioningStrategy<TMessage> _partitioningStrategy;
    private readonly IBus _bus;

    internal static string _partitionBaseUri; // This static field in generic class is initialized when registering in dependency injection.

    public PartitionerPublisher(
        Bind<IMassTransitPartitionerPublisher, IBus> bindedBus,
        PartitioningStrategy<TMessage> partitioningStrategy)
    {
        _partitioningStrategy = partitioningStrategy;
        _bus = bindedBus.Value;
    }

    public async Task Publish(TMessage message)
    {
        ISendEndpoint sendEndpoint = await _bus.GetSendEndpoint(new Uri(
                _partitionBaseUri + _partitioningStrategy.ExtractPartitionFromMessage(message,
                _partitioningStrategy.PartitionsCount)));
        await sendEndpoint.Send(message);
    }
}