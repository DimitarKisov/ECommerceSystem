using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagement.Domain.Events;
using OrderManagement.Infrastructure.Messaging;
using Shared.Contracts.Events;

namespace OrderManagement.Infrastructure.EventHandlers
{
    /// <summary>
    /// Handler за OrderDeliveredEvent domain event
    /// </summary>
    public class OrderDeliveredEventHandler : INotificationHandler<OrderDeliveredEvent>
    {
        private readonly IMessageBus _messageBus;
        private readonly ILogger<OrderDeliveredEventHandler> _logger;

        public OrderDeliveredEventHandler(IMessageBus messageBus, ILogger<OrderDeliveredEventHandler> logger)
        {
            _messageBus = messageBus;
            _logger = logger;
        }

        public async Task Handle(OrderDeliveredEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Handling OrderDeliveredEvent for Order {OrderId}",
                notification.OrderId);

            try
            {
                var integrationEvent = new OrderDeliveredIntegrationEvent
                {
                    EventId = notification.EventId,
                    OccurredOn = notification.OccurredOn,
                    OrderId = notification.OrderId,
                    CustomerId = notification.CustomerId,
                    DeliveredDate = notification.DeliveredDate
                };

                // Routing key: order.delivered
                await _messageBus.PublishAsync(
                    integrationEvent,
                    "order.delivered",
                    cancellationToken);

                _logger.LogInformation(
                    "Successfully published OrderDeliveredIntegrationEvent for Order {OrderId}",
                    notification.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error publishing OrderDeliveredIntegrationEvent for Order {OrderId}",
                    notification.OrderId);
            }
        }
    }
}