using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Events
{
    /// <summary>
    /// Domain event за създадена поръчка
    /// </summary>
    public record OrderCreatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid OrderId { get; }
        public Guid CustomerId { get; }
        public DateTime OrderDate { get; }

        public OrderCreatedEvent(Guid orderId, Guid customerId, DateTime orderDate)
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            OrderId = orderId;
            CustomerId = customerId;
            OrderDate = orderDate;
        }
    }
}
