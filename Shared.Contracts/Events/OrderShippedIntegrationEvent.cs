namespace Shared.Contracts.Events
{
    /// <summary>
    /// Integration event за изпратена поръчка
    /// </summary>
    public class OrderShippedIntegrationEvent
    {
        public Guid EventId { get; set; }
        public DateTime OccurredOn { get; set; }
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime ShippedDate { get; set; }
    }
}