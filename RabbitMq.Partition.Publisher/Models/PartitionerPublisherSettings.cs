using System.Collections.Generic;
using RabbitMq.Partition.Publisher.Enums;

namespace RabbitMq.Partition.Publisher.Models;

public class PartitionPublisherSettings
{
    public string? ConnectionString { get; set; } = null;

    public PersistenceProvider PersistenceProvider { get; set; } = PersistenceProvider.None;

    public List<Topic> Topics { get; } = new List<Topic>();
}