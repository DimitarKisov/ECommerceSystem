namespace Shared.Contracts.Events
{
    /// <summary>
    /// Integration event за доставена поръчка
    /// </summary>
    public class OrderDeliveredIntegrationEvent
    {
        public Guid EventId { get; set; }
        public DateTime OccurredOn { get; set; }
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime DeliveredDate { get; set; }
    }
}