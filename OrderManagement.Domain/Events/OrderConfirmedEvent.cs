using OrderManagement.Domain.Common;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Domain.Events
{
    /// <summary>
    /// Domain event за потвърдена поръчка
    /// Този event ще triggerne процеси за: проверка на наличности, резервация на продукти, payment
    /// </summary>
    public record OrderConfirmedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid OrderId { get; }
        public Guid CustomerId { get; }
        public Money TotalAmount { get; }
        public List<OrderItemDto> Items { get; }

        public OrderConfirmedEvent(Guid orderId, Guid customerId, Money totalAmount, List<OrderItem> items)
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            OrderId = orderId;
            CustomerId = customerId;
            TotalAmount = totalAmount;

            // Конвертираме към DTO за да не leak-ваме domain entities извън aggregate
            Items = items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice.Amount
            }).ToList();
        }

        public record OrderItemDto
        {
            public Guid ProductId { get; init; }
            public string ProductName { get; init; } = null!;
            public int Quantity { get; init; }
            public decimal UnitPrice { get; init; }
        }
    }
}
