using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.AspNetCore.Mvc;
using RabbitMq.Partitioner.Abstractions;
using RabbitMq.Partitioner.Tests.Events;

namespace RabbitMq.Partitioner.Tests.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartitionerTestingController : ControllerBase
{
    private readonly IPartitionPublisher _publisher;

    public PartitionerTestingController(IPartitionPublisher publisher)
    {
        _publisher = publisher;
    }

    [HttpPost]
    public async Task<IActionResult> PublishTestStringEvent([FromBody] TestStringEvent @event, CancellationToken cancellationToken)
    {
        await _publisher.PublishAsync(@event, "Test", cancellationToken);
        return Ok();
    }
}