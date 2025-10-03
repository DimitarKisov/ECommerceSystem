namespace Shared.Contracts.Events
{
    /// <summary>
    /// Integration event за отменена поръчка
    /// Това освобождава резервирани продукти в Inventory
    /// </summary>
    public class OrderCancelledIntegrationEvent
    {
        public Guid EventId { get; set; }
        public DateTime OccurredOn { get; set; }
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public string CancellationReason { get; set; } = string.Empty;
        public string PreviousStatus { get; set; } = string.Empty;
    }
}