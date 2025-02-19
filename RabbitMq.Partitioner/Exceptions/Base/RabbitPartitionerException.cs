namespace RabbitMq.Partitioner.Exceptions.Base;

public class RabbitPartitionerException : Exception
{
    public RabbitPartitionerException(string message) : base(message) { }
}