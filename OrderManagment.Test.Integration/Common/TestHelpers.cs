using OrderManagement.Domain.Entities;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Tests.Integration.Common
{
    /// <summary>
    /// Helper методи за създаване на test данни
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// Създава тестова поръчка с параметри по подразбиране
        /// </summary>
        public static Order CreateTestOrder(
            Guid? customerId = null,
            Address? shippingAddress = null,
            int itemsCount = 1)
        {
            var order = Order.Create(
                customerId ?? Guid.NewGuid(),
                shippingAddress ?? CreateTestAddress());

            for (int i = 0; i < itemsCount; i++)
            {
                order.AddItem(
                    Guid.NewGuid(),
                    $"Test Product {i + 1}",
                    new Money(100m * (i + 1)),
                    1);
            }

            return order;
        }

        /// <summary>
        /// Създава тестов адрес
        /// </summary>
        public static Address CreateTestAddress(
            string street = "ул. Витоша 100",
            string city = "София",
            string postalCode = "1000",
            string country = "България")
        {
            return new Address(street, city, postalCode, country);
        }

        /// <summary>
        /// Създава тестова поръчка със статус Confirmed
        /// </summary>
        public static Order CreateConfirmedOrder(
            Guid? customerId = null,
            int itemsCount = 1)
        {
            var order = CreateTestOrder(customerId, null, itemsCount);
            order.Confirm();
            order.ClearDomainEvents(); // Изчистваме events за по-clean тестове
            return order;
        }

        /// <summary>
        /// Създава тестова поръчка със статус Shipped
        /// </summary>
        public static Order CreateShippedOrder(
            Guid? customerId = null,
            int itemsCount = 1)
        {
            var order = CreateConfirmedOrder(customerId, itemsCount);
            order.MarkAsShipped();
            order.ClearDomainEvents();
            return order;
        }

        /// <summary>
        /// Създава тестова поръчка със статус Delivered
        /// </summary>
        public static Order CreateDeliveredOrder(
            Guid? customerId = null,
            int itemsCount = 1)
        {
            var order = CreateShippedOrder(customerId, itemsCount);
            order.MarkAsDelivered();
            order.ClearDomainEvents();
            return order;
        }

        /// <summary>
        /// Създава тестова поръчка със статус Cancelled
        /// </summary>
        public static Order CreateCancelledOrder(
            Guid? customerId = null,
            string reason = "Test cancellation")
        {
            var order = CreateTestOrder(customerId);
            order.Cancel(reason);
            order.ClearDomainEvents();
            return order;
        }

        /// <summary>
        /// Генерира рандомен email
        /// </summary>
        public static string GenerateRandomEmail()
        {
            return $"test{Guid.NewGuid():N}@example.com";
        }

        /// <summary>
        /// Генерира рандомно име на продукт
        /// </summary>
        public static string GenerateRandomProductName()
        {
            var products = new[]
            {
                "Laptop", "Mouse", "Keyboard", "Monitor", "Headphones",
                "Webcam", "Microphone", "Speakers", "Tablet", "Phone"
            };
            var random = new Random();
            return products[random.Next(products.Length)] + " " + random.Next(1000);
        }
    }
}