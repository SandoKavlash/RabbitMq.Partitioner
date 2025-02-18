using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using RabbitMq.Partition.Publisher.Abstractions;
using RabbitMq.Partition.Publisher.Models;

namespace RabbitMq.Partition.Publisher.Implementations;

internal class PartitionPublisherStraightToTheRabbit : IPartitionPublisher
{
    private readonly IPartitionBus _partitionBus;

    public PartitionPublisherStraightToTheRabbit(IPartitionBus partitionBus)
    {
        _partitionBus = partitionBus;
    }
    public async Task PublishGuidEventAsync<TMessage>(TMessage data, CancellationToken cancellationToken = default)
        where TMessage : class, IPartitionedEventByGuid
    {
        cancellationToken.ThrowIfCancellationRequested();

        string? topic = Topic<TMessage>.TopicNameCache;
        if (topic == null) throw new ArgumentException($"Event type: {typeof(TMessage)} is not configured");

        if (!Constants.TopicsAndPartitionsMapping.TryGetValue(topic, out int partitionsCount))
        {
            throw new ArgumentException("Topic that you provided, is not registered in this application");
        }

        int partitionId = CalculatePartitionId(data, partitionsCount);

        await _partitionBus.Publish(data, context =>
        {
            context.Durable = true;
            context.SetRoutingKey($"{topic}-{partitionId}");
        },cancellationToken);
    }

    private int CalculatePartitionId(IPartitionedEventByGuid data, int numberOfPartitions)
    {
        using var sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data.PartitionKey.ToString()));
        
        // Convert first 8 bytes of hash to a long
        long hashLong = BitConverter.ToInt64(hashBytes, 0);

        if (hashLong < 0) hashLong = -hashLong;
    
        return (int)(hashLong % numberOfPartitions);
    }

    private int CalculatePartitionId(IPartitionedEventByString data, int numberOfPartitions)
    {
        using var sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data.PartitionKey));
        
        // Convert first 8 bytes of hash to a long
        long hashLong = BitConverter.ToInt64(hashBytes, 0);

        if (hashLong < 0) hashLong = -hashLong;
    
        return (int)(hashLong % numberOfPartitions);
    }

    public async Task PublishStringEventAsync<TMessage>(TMessage data, CancellationToken cancellationToken = default)
        where TMessage : class, IPartitionedEventByString
    {
        cancellationToken.ThrowIfCancellationRequested();

        string? topic = Topic<TMessage>.TopicNameCache;
        if (topic == null) throw new ArgumentException($"Event type: {typeof(TMessage)} is not configured");

        if (!Constants.TopicsAndPartitionsMapping.TryGetValue(topic, out int partitionsCount))
        {
            throw new ArgumentException("Topic that you provided, is not registered in this application");
        }

        int partitionId = CalculatePartitionId(data, partitionsCount);

        await _partitionBus.Publish(data, context =>
        {
            context.Durable = true;
            context.SetRoutingKey($"{topic}-{partitionId}");
        },cancellationToken);
    }
}