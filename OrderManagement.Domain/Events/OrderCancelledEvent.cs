using OrderManagement.Domain.Common;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Events
{
    /// <summary>
    /// Domain event за отменена поръчка
    /// Този event ще освобождава резервирани продукти в Inventory
    /// </summary>
    public record OrderCancelledEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid OrderId { get; }
        public Guid CustomerId { get; }
        public string CancellationReason { get; }
        public OrderStatus PreviousStatus { get; }

        public OrderCancelledEvent(
            Guid orderId,
            Guid customerId,
            string cancellationReason,
            OrderStatus previousStatus)
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            OrderId = orderId;
            CustomerId = customerId;
            CancellationReason = cancellationReason;
            PreviousStatus = previousStatus;
        }
    }
}
