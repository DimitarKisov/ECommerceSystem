using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Events
{
    /// <summary>
    /// Domain event за изпратена поръчка
    /// </summary>
    public record OrderShippedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid OrderId { get; }
        public Guid CustomerId { get; }
        public DateTime ShippedDate { get; }

        public OrderShippedEvent(Guid orderId, Guid customerId, DateTime shippedDate)
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            OrderId = orderId;
            CustomerId = customerId;
            ShippedDate = shippedDate;
        }
    }
}
