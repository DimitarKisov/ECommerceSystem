using OrderManagement.Domain.Common;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Events;
using OrderManagement.Domain.ValueObjetcs;

namespace OrderManagement.Domain.Entities
{
    /// <summary>
    /// Агрегат root за поръчка
    /// </summary>
    public class Order : BaseEntity
    {
        private readonly List<OrderItem> _items = new();

        public string OrderNumber { get; private set; } = null!;
        public Guid CustomerId { get; private set; }
        public DateTime OrderDate { get; private set; }
        public OrderStatus Status { get; private set; }
        public Address ShippingAddress { get; private set; } = null!;
        public Money TotalAmount { get; private set; } = null!;
        public DateTime? ShippedDate { get; private set; }
        public DateTime? DeliveredDate { get; private set; }
        public string CancellationReason { get; private set; }

        /// <summary>
        /// Read-only колекция от продукти в поръчката
        /// </summary>
        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

        // Private constructor за EF Core
        private Order() { }

        /// <summary>
        /// Създава нова поръчка
        /// </summary>
        public static Order Create(Guid customerId, Address shippingAddress)
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = GenerateOrderNumber(),
                CustomerId = customerId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                ShippingAddress = shippingAddress,
                TotalAmount = new Money(0)
            };

            // Добавяме domain event
            order.AddDomainEvent(new OrderCreatedEvent(order.Id, order.CustomerId, order.OrderDate));

            return order;
        }

        /// <summary>
        /// Добавя продукт към поръчката
        /// </summary>
        public void AddItem(Guid productId, string productName, Money unitPrice, int quantity)
        {
            if (Status != OrderStatus.Pending)
            {
                throw new InvalidOperationException("Не може да се добавят продукти към потвърдена поръчка");
            }

            if (quantity <= 0)
            {
                throw new ArgumentException("Количеството трябва да е положително число", nameof(quantity));
            }

            // Проверка дали продуктът вече съществува
            var existingItem = _items.FirstOrDefault(x => x.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.IncreaseQuantity(quantity);
            }
            else
            {
                var item = OrderItem.Create(Id, productId, productName, unitPrice, quantity);
                _items.Add(item);
            }

            RecalculateTotalAmount();
        }

        /// <summary>
        /// Премахва продукт от поръчката
        /// </summary>
        public void RemoveItem(Guid productId)
        {
            if (Status != OrderStatus.Pending)
            {
                throw new InvalidOperationException("Не може да се премахват продукти от потвърдена поръчка");
            }

            var item = _items.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                _items.Remove(item);
                RecalculateTotalAmount();
            }
        }

        /// <summary>
        /// Потвърждава поръчката
        /// </summary>
        public void Confirm()
        {
            if (Status != OrderStatus.Pending)
            {
                throw new InvalidOperationException($"Не може да се потвърди поръчка със статус {Status}");
            }

            if (!_items.Any())
            {
                throw new InvalidOperationException("Не може да се потвърди празна поръчка");
            }

            Status = OrderStatus.Confirmed;

            // Domain event за потвърдена поръчка
            AddDomainEvent(new OrderConfirmedEvent(Id, CustomerId, TotalAmount, _items.ToList()));
        }

        /// <summary>
        /// Отказва поръчката
        /// </summary>
        public void Cancel(string reason)
        {
            if (Status == OrderStatus.Delivered || Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException($"Не може да се отмени поръчка със статус {Status}");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("Причината за отмяна е задължителна", nameof(reason));
            }

            var previousStatus = Status;
            Status = OrderStatus.Cancelled;
            CancellationReason = reason;

            // Domain event за отменена поръчка
            AddDomainEvent(new OrderCancelledEvent(Id, CustomerId, reason, previousStatus));
        }

        /// <summary>
        /// Маркира поръчката като изпратена
        /// </summary>
        public void MarkAsShipped()
        {
            if (Status != OrderStatus.Confirmed && Status != OrderStatus.Processing)
            {
                throw new InvalidOperationException($"Не може да се изпрати поръчка със статус {Status}");
            }

            Status = OrderStatus.Shipped;
            ShippedDate = DateTime.UtcNow;

            AddDomainEvent(new OrderShippedEvent(Id, CustomerId, ShippedDate.Value));
        }

        /// <summary>
        /// Маркира поръчката като доставена
        /// </summary>
        public void MarkAsDelivered()
        {
            if (Status != OrderStatus.Shipped)
            {
                throw new InvalidOperationException($"Не може да се достави поръчка със статус {Status}");
            }

            Status = OrderStatus.Delivered;
            DeliveredDate = DateTime.UtcNow;

            AddDomainEvent(new OrderDeliveredEvent(Id, CustomerId, DeliveredDate.Value));
        }

        /// <summary>
        /// Преизчислява общата сума на поръчката
        /// </summary>
        private void RecalculateTotalAmount()
        {
            var total = _items.Sum(x => x.Subtotal.Amount);
            TotalAmount = new Money(total, "BGN");
        }

        /// <summary>
        /// Генерира уникален номер на поръчка
        /// </summary>
        private static string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";
        }
    }
}
