using System;
using System.Security.Cryptography;
using System.Text;

namespace RabbitMq.Partitioner.Publisher.Models;

public static class PartitioningStrategy
{
    public static int DefaultPartitioner(string messageIdentifier, int partitionsCount)
    {
        if (string.IsNullOrEmpty(messageIdentifier))
            throw new ArgumentException("Identifier cannot be null or empty", nameof(messageIdentifier));

        if (partitionsCount <= 0)
            throw new ArgumentException("Partitions count must be greater than zero", nameof(partitionsCount));

        // Compute hash of the identifier
        using (var sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(messageIdentifier));

            // Convert first 4 bytes of hash to an integer
            int hashValue = BitConverter.ToInt32(hashBytes, 0);

            if (hashValue < 0) hashValue *= -1;

            // Return partition ID
            return hashValue % partitionsCount;
        }
    }

    public static int DefaultPartitioner(int messageIdentifier, int partitionCount)
    {
        return messageIdentifier % partitionCount;
    }
}

public class PartitioningStrategy<TMessage> 
    where TMessage : class
{
    public required string TopicName { get; set; }
    
    public required int PartitionsCount { get; set; }

    /// <summary>
    /// This Func is partitioner which takes message and partitionCount as parameter and returns partition_id
    /// </summary>
    public required Func<TMessage, int, int> ExtractPartitionFromMessage { get; set; }
}