using System.Threading.Tasks;

namespace RabbitMq.Partitioner.Publisher.Abstractions;

public interface IPartitionerPublisher<in TMessage> where TMessage : class
{
    Task Publish(TMessage message);
}