using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace RabbitMq.Partition.Publisher.Abstractions.Jobs;

internal class OutboxPublisherJob : BackgroundService
{
    private readonly PartitionPublisherSettings _settings;

    internal OutboxPublisherJob(PartitionPublisherSettings settings)
    {
        _settings = settings;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new System.NotImplementedException();
    }
}