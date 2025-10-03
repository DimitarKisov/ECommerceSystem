using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace OrderManagement.Infrastructure.Messaging
{
    /// <summary>
    /// RabbitMQ имплементация на message bus
    /// </summary>
    public class RabbitMqMessageBus : IMessageBus, IDisposable
    {
        private readonly ILogger<RabbitMqMessageBus> _logger;
        private readonly IConnection _connection;
        private readonly RabbitMQ.Client.IModel _channel;
        private readonly string _exchangeName;

        public RabbitMqMessageBus(IConfiguration configuration, ILogger<RabbitMqMessageBus> logger)
        {
            _logger = logger;

            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:Host"] ?? "localhost",
                Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = configuration["RabbitMQ:Username"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest"
            };

            _exchangeName = configuration["RabbitMQ:ExchangeName"] ?? "ecommerce_exchange";

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Декларираме exchange
                _channel.ExchangeDeclare(
                    exchange: _exchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false);

                _logger.LogInformation("RabbitMQ connection established successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to RabbitMQ");
                throw;
            }
        }

        public Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.ContentType = "application/json";
                properties.Type = typeof(T).Name;

                _channel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation(
                    "Published message {MessageType} with routing key {RoutingKey}",
                    typeof(T).Name,
                    routingKey);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message to RabbitMQ");
                throw;
            }
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}