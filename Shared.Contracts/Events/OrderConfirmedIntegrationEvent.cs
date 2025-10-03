namespace Shared.Contracts.Events
{
    /// <summary>
    /// Integration event за потвърдена поръчка
    /// Това се изпраща към Inventory и Payment микросервизите
    /// </summary>
    public class OrderConfirmedIntegrationEvent
    {
        public Guid EventId { get; set; }
        public DateTime OccurredOn { get; set; }
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "BGN";
        public List<OrderItemDto> Items { get; set; } = new();

        public class OrderItemDto
        {
            public Guid ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
        }
    }
}