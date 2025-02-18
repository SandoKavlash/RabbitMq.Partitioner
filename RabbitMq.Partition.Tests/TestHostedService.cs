using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMq.Partition.Publisher.Abstractions;

namespace RabbitMq.Partition.Tests;

public class TestHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public TestHostedService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            IPartitionPublisher publisher = scope.ServiceProvider.GetRequiredService<IPartitionPublisher>();
            for (int i = 0; i < 100; i++)
            {
                await publisher.PublishStringEventAsync(new TestEvent()
                {
                    Message = "Sandrikela" + i
                }, stoppingToken);
                await Task.Delay(100);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}

public class TestEvent : IPartitionedEventByString
{
    public string Message { get; set; }

    public string PartitionKey => Message;
}