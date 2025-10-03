namespace OrderManagement.Infrastructure.Messaging
{
    /// <summary>
    /// Интерфейс за message bus (RabbitMQ)
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// Публикува съобщение към message bus
        /// </summary>
        Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default) where T : class;
    }
}