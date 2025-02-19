using System.Security.Cryptography;
using System.Text;
using EasyNetQ;
using EasyNetQ.Topology;
using RabbitMq.Partitioner.Abstractions;
using RabbitMq.Partitioner.Exceptions.Base;
using RabbitMq.Partitioner.Models;

namespace RabbitMq.Partitioner.Implementations;

public class PartitionPublisherDirectlyInRabbit : IPartitionPublisher
{
    private readonly IBus _bus;

    private readonly IAdvancedBus _advancedBus;

    private readonly MessageProperties _messageProperties;

    public PartitionPublisherDirectlyInRabbit(IBus bus)
    {
        _bus = bus;
        _advancedBus = bus.Advanced;
        _messageProperties = new MessageProperties
        {
            DeliveryMode = 2, // it means that message must be durable.
        };
    }

    public Task PublishAsync(IGuidPartitionKeyable @event, string topicName, CancellationToken cancellationToken = default)
    {
        if (@event == null) throw new RabbitPartitionerException("Cannot publish null message");

        Topic topic = EnsureTopicConfigurationExists(topicName);

        Exchange topicExchange = EnsureTopicExchangeIsConfigured(topicName, topic);

        string routingKey = CalculateRoutingKey(@event, topic);

        return _advancedBus.PublishAsync(
            exchange: topicExchange.Name, 
            routingKey: routingKey, 
            mandatory: false, 
            message: new Message<IGuidPartitionKeyable>(@event, _messageProperties), 
            cancellationToken: cancellationToken);
    }

    public Task PublishAsync(IStringPartitionKeyable @event, string topicName, CancellationToken cancellationToken = default)
    {
        if (@event == null) throw new RabbitPartitionerException("Cannot publish null message");

        Topic topic = EnsureTopicConfigurationExists(topicName);

        Exchange topicExchange = EnsureTopicExchangeIsConfigured(topicName, topic);

        string routingKey = CalculateRoutingKey(@event, topic);

        return _advancedBus.PublishAsync(
            exchange: topicExchange.Name, 
            routingKey: routingKey, 
            mandatory: false, 
            message: new Message<IStringPartitionKeyable>(@event, _messageProperties), 
            cancellationToken: cancellationToken);
    }

    private static Topic EnsureTopicConfigurationExists(string topicName)
    {
        if (!Constants.Topics.TryGetValue(topicName, out Topic? topic))
        {
            throw new RabbitPartitionerException($"your requested topic: '{topicName}' was not registered");
        }

        return topic;
    }

    private static Exchange EnsureTopicExchangeIsConfigured(string topicName, Topic topic)
    {
        Exchange? topicExchange = topic.CreatedExchange;
        if (topicExchange == null)
        {
            throw new RabbitPartitionerException(
                $"Somehow your provided topicName '{topicName}' was not configured in RabbitMq");
        }

        return topicExchange.Value;
    }

    private string CalculateRoutingKey(IGuidPartitionKeyable @event, Topic topic)
    {
        int partitionId = CalculatePartitionId(@event, topic.PartitionsCount);
        return $"{topic.Name}-{partitionId}";
    }

    private string CalculateRoutingKey(IStringPartitionKeyable @event, Topic topic)
    {
        int partitionId = CalculatePartitionId(@event, topic.PartitionsCount);
        return $"{topic.Name}-{partitionId}";
    }

    private int CalculatePartitionId(IGuidPartitionKeyable data, int numberOfPartitions)
    {
        using var sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data.PartitionKey.ToString()));

        // Convert first 8 bytes of hash to a long
        long hashLong = BitConverter.ToInt64(hashBytes, 0);

        if (hashLong < 0) hashLong = -hashLong;

        return (int)(hashLong % numberOfPartitions);
    }

    private int CalculatePartitionId(IStringPartitionKeyable data, int numberOfPartitions)
    {
        using var sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data.PartitionKey));

        // Convert first 8 bytes of hash to a long
        long hashLong = BitConverter.ToInt64(hashBytes, 0);

        if (hashLong < 0) hashLong = -hashLong;

        return (int)(hashLong % numberOfPartitions);
    }
}