using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagement.Domain.Events;
using OrderManagement.Infrastructure.Messaging;
using Shared.Contracts.Events;

namespace OrderManagement.Infrastructure.EventHandlers
{
    /// <summary>
    /// Handler за OrderCancelledEvent domain event
    /// </summary>
    public class OrderCancelledEventHandler : INotificationHandler<OrderCancelledEvent>
    {
        private readonly IMessageBus _messageBus;
        private readonly ILogger<OrderCancelledEventHandler> _logger;

        public OrderCancelledEventHandler(IMessageBus messageBus, ILogger<OrderCancelledEventHandler> logger)
        {
            _messageBus = messageBus;
            _logger = logger;
        }

        public async Task Handle(OrderCancelledEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Handling OrderCancelledEvent for Order {OrderId}",
                notification.OrderId);

            try
            {
                var integrationEvent = new OrderCancelledIntegrationEvent
                {
                    EventId = notification.EventId,
                    OccurredOn = notification.OccurredOn,
                    OrderId = notification.OrderId,
                    CustomerId = notification.CustomerId,
                    CancellationReason = notification.CancellationReason,
                    PreviousStatus = notification.PreviousStatus.ToString()
                };

                // Routing key: order.cancelled (Inventory ще освободи резервираните продукти)
                await _messageBus.PublishAsync(
                    integrationEvent,
                    "order.cancelled",
                    cancellationToken);

                _logger.LogInformation(
                    "Successfully published OrderCancelledIntegrationEvent for Order {OrderId}",
                    notification.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error publishing OrderCancelledIntegrationEvent for Order {OrderId}",
                    notification.OrderId);
            }
        }
    }
}