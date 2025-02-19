using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RabbitMq.Partitioner;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi("docs");

builder.Services.AddPartitioner((config) =>
{
    config
        .ConfigureTopic(topicConfig => topicConfig
            .WithName("Test")
            .WithPartitionsCount(3))
        .ConfigureTopic(topicConfig => topicConfig
            .WithName("GameEvents")
            .WithPartitionsCount(12));
}, "host=localhost;virtualHost=2;username=root;password=root;port=5672;publisherConfirms=true");

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/docs.json", "LionBitcoin.Payments.Service");
});

app.MapControllers();

app.UsePartitioner();
app.Run();