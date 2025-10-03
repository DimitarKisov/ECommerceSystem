using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagement.Domain.Events;
using OrderManagement.Infrastructure.Messaging;
using Shared.Contracts.Events;

namespace OrderManagement.Infrastructure.EventHandlers
{
    /// <summary>
    /// Handler за OrderShippedEvent domain event
    /// </summary>
    public class OrderShippedEventHandler : INotificationHandler<OrderShippedEvent>
    {
        private readonly IMessageBus _messageBus;
        private readonly ILogger<OrderShippedEventHandler> _logger;

        public OrderShippedEventHandler(IMessageBus messageBus, ILogger<OrderShippedEventHandler> logger)
        {
            _messageBus = messageBus;
            _logger = logger;
        }

        public async Task Handle(OrderShippedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Handling OrderShippedEvent for Order {OrderId}",
                notification.OrderId);

            try
            {
                var integrationEvent = new OrderShippedIntegrationEvent
                {
                    EventId = notification.EventId,
                    OccurredOn = notification.OccurredOn,
                    OrderId = notification.OrderId,
                    CustomerId = notification.CustomerId,
                    ShippedDate = notification.ShippedDate
                };

                // Routing key: order.shipped
                await _messageBus.PublishAsync(
                    integrationEvent,
                    "order.shipped",
                    cancellationToken);

                _logger.LogInformation(
                    "Successfully published OrderShippedIntegrationEvent for Order {OrderId}",
                    notification.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error publishing OrderShippedIntegrationEvent for Order {OrderId}",
                    notification.OrderId);
            }
        }
    }
}