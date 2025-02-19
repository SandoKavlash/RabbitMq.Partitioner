using RabbitMq.Partitioner;
using RabbitMq.Partitioner.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddPartitioner((config) =>
{
    config.AddTopic(new Topic()
    {
        Name = "Test",
        PartitionsCount = 3
    });
    config.AddTopic(new Topic()
    {
        Name = "GameEvents",
        PartitionsCount = 12
    });
}, "host=localhost;virtualHost=2;username=root;password=root;port=5672");

var app = builder.Build();


app.UsePartitioner();
app.Run();