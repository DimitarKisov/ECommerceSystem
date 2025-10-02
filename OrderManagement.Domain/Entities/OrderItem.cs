using OrderManagement.Domain.Common;
using OrderManagement.Domain.ValueObjetcs;

namespace OrderManagement.Domain.Entities
{
    /// <summary>
    /// Entity за продукт в поръчка
    /// </summary>
    public class OrderItem : BaseEntity
    {
        public Guid OrderId { get; private set; }
        public Guid ProductId { get; private set; }
        public string ProductName { get; private set; }
        public Money UnitPrice { get; private set; }
        public int Quantity { get; private set; }
        public Money Subtotal { get; private set; }

        // Private constructor за EF Core
        private OrderItem() { }

        /// <summary>
        /// Създава нов ред в поръчката
        /// </summary>
        public static OrderItem Create(Guid orderId, Guid productId, string productName, Money unitPrice, int quantity)
        {
            if (quantity <= 0)
            {
                throw new ArgumentException("Количеството трябва да е положително число", nameof(quantity));
            }

            var item = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ProductId = productId,
                ProductName = productName,
                UnitPrice = unitPrice,
                Quantity = quantity
            };

            item.CalculateSubtotal();
            return item;
        }

        /// <summary>
        /// Увеличава количеството
        /// </summary>
        public void IncreaseQuantity(int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Количеството трябва да е положително число", nameof(amount));
            }

            Quantity += amount;
            CalculateSubtotal();
        }

        /// <summary>
        /// Променя количеството
        /// </summary>
        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
            {
                throw new ArgumentException("Количеството трябва да е положително число", nameof(newQuantity));
            }

            Quantity = newQuantity;
            CalculateSubtotal();
        }

        /// <summary>
        /// Изчислява междинната сума
        /// </summary>
        private void CalculateSubtotal()
        {
            Subtotal = UnitPrice.Multiply(Quantity);
        }
    }
}
