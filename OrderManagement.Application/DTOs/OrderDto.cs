namespace OrderManagement.Application.DTOs
{
    // ========================================
    // DTOs за Query методите
    // ========================================

    public class OrderDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public AddressDto ShippingAddress { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "BGN";
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public string CancellationReason { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }
}
