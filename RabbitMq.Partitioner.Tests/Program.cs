using RabbitMq.Partitioner;
using RabbitMq.Partitioner.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddPartitioner((config) =>
{
    config
        .ConfigureTopic(topicConfig => topicConfig
            .WithName("Test")
            .WithPartitionsCount(3))
        .ConfigureTopic(topicConfig => topicConfig
            .WithName("GameEvents")
            .WithPartitionsCount(12));
}, "host=localhost;virtualHost=2;username=root;password=root;port=5672");

var app = builder.Build();


app.UsePartitioner();
app.Run();