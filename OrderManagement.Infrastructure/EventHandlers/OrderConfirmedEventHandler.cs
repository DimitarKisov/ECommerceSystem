using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Common;
using OrderManagement.Domain.Events;
using OrderManagement.Infrastructure.Messaging;
using Shared.Contracts.Events;

namespace OrderManagement.Infrastructure.EventHandlers
{
    /// <summary>
    /// Handler за OrderConfirmedEvent domain event
    /// Превръща domain event в integration event и го публикува към RabbitMQ
    /// </summary>
    public class OrderConfirmedEventHandler : INotificationHandler<OrderConfirmedEvent>
    {
        private readonly IMessageBus _messageBus;
        private readonly ILogger<OrderConfirmedEventHandler> _logger;

        public OrderConfirmedEventHandler(IMessageBus messageBus, ILogger<OrderConfirmedEventHandler> logger)
        {
            _messageBus = messageBus;
            _logger = logger;
        }

        public async Task Handle(OrderConfirmedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Handling OrderConfirmedEvent for Order {OrderId}",
                notification.OrderId);

            try
            {
                // Превръщаме domain event в integration event
                var integrationEvent = new OrderConfirmedIntegrationEvent
                {
                    EventId = notification.EventId,
                    OccurredOn = notification.OccurredOn,
                    OrderId = notification.OrderId,
                    CustomerId = notification.CustomerId,
                    TotalAmount = notification.TotalAmount.Amount,
                    Currency = notification.TotalAmount.Currency,
                    Items = notification.Items.Select(i => new OrderConfirmedIntegrationEvent.OrderItemDto
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList()
                };

                // Публикуваме към RabbitMQ
                // Routing key: order.confirmed (Inventory и Payment ще слушат за този key)
                await _messageBus.PublishAsync(
                    integrationEvent,
                    "order.confirmed",
                    cancellationToken);

                _logger.LogInformation(
                    "Successfully published OrderConfirmedIntegrationEvent for Order {OrderId}",
                    notification.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error publishing OrderConfirmedIntegrationEvent for Order {OrderId}",
                    notification.OrderId);

                // Не пропагираме грешката, за да не се провали transaction-а
                // В production: retry mechanism, dead letter queue, etc.
            }
        }
    }
}