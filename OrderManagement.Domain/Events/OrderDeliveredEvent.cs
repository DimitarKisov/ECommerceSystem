using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Events
{
    /// <summary>
    /// Domain event за доставена поръчка
    /// </summary>
    public record OrderDeliveredEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid OrderId { get; }
        public Guid CustomerId { get; }
        public DateTime DeliveredDate { get; }

        public OrderDeliveredEvent(Guid orderId, Guid customerId, DateTime deliveredDate)
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            OrderId = orderId;
            CustomerId = customerId;
            DeliveredDate = deliveredDate;
        }
    }
}
