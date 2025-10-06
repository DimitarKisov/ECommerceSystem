using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Infrastructure.Data.SeedData
{
    /// <summary>
    /// Клас за seed на тестови данни в базата
    /// </summary>
    public static class OrderSeeder
    {
        /// <summary>
        /// Seed на тестови поръчки
        /// </summary>
        public static async Task SeedAsync(OrderManagementDbContext context, ILogger logger)
        {
            try
            {
                // Проверяваме дали вече има данни
                if (await context.Orders.AnyAsync())
                {
                    logger.LogInformation("База данните вече съдържа поръчки. Seed се пропуска.");
                    return;
                }

                logger.LogInformation("Започване на seed на тестови данни...");

                // Тестови Customer IDs
                var customer1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
                var customer2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
                var customer3Id = Guid.Parse("33333333-3333-3333-3333-333333333333");

                // Тестови Product IDs
                var laptopId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
                var mouseId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
                var keyboardId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
                var monitorId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
                var headphonesId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");

                var orders = new List<Order>();

                // Поръчка 1: Pending (нова поръчка)
                var order1 = Order.Create(
                    customer1Id,
                    new Address("ул. Витоша 100", "София", "1000", "България")
                );
                order1.AddItem(laptopId, "Lenovo ThinkPad X1 Carbon", new Money(2499.99m), 1);
                order1.AddItem(mouseId, "Logitech MX Master 3", new Money(99.99m), 1);
                orders.Add(order1);

                // Поръчка 2: Confirmed (потвърдена, готова за обработка)
                var order2 = Order.Create(
                    customer2Id,
                    new Address("бул. Христо Ботев 25", "Пловдив", "4000", "България")
                );
                order2.AddItem(monitorId, "Dell UltraSharp 27\" 4K", new Money(549.99m), 2);
                order2.AddItem(keyboardId, "Keychron K8 Pro", new Money(129.99m), 1);
                order2.Confirm();
                orders.Add(order2);

                // Поръчка 3: Shipped (изпратена)
                var order3 = Order.Create(
                    customer1Id,
                    new Address("ул. Витоша 100", "София", "1000", "България")
                );
                order3.AddItem(headphonesId, "Sony WH-1000XM5", new Money(399.99m), 1);
                order3.Confirm();
                order3.MarkAsShipped();
                orders.Add(order3);

                // Поръчка 4: Delivered (доставена)
                var order4 = Order.Create(
                    customer3Id,
                    new Address("ул. Александър Стамболийски 52", "Варна", "9000", "България")
                );
                order4.AddItem(laptopId, "Lenovo ThinkPad X1 Carbon", new Money(2499.99m), 1);
                order4.AddItem(mouseId, "Logitech MX Master 3", new Money(99.99m), 1);
                order4.AddItem(keyboardId, "Keychron K8 Pro", new Money(129.99m), 1);
                order4.Confirm();
                order4.MarkAsShipped();
                order4.MarkAsDelivered();
                orders.Add(order4);

                // Поръчка 5: Cancelled (отменена)
                var order5 = Order.Create(
                    customer2Id,
                    new Address("бул. Христо Ботев 25", "Пловдив", "4000", "България")
                );
                order5.AddItem(monitorId, "Dell UltraSharp 27\" 4K", new Money(549.99m), 1);
                order5.Cancel("Клиентът промени решението си");
                orders.Add(order5);

                // Поръчка 6: Голяма поръчка - Confirmed
                var order6 = Order.Create(
                    customer3Id,
                    new Address("ул. Александър Стамболийски 52", "Варна", "9000", "България")
                );
                order6.AddItem(laptopId, "Lenovo ThinkPad X1 Carbon", new Money(2499.99m), 5);
                order6.AddItem(monitorId, "Dell UltraSharp 27\" 4K", new Money(549.99m), 5);
                order6.AddItem(mouseId, "Logitech MX Master 3", new Money(99.99m), 5);
                order6.AddItem(keyboardId, "Keychron K8 Pro", new Money(129.99m), 5);
                order6.AddItem(headphonesId, "Sony WH-1000XM5", new Money(399.99m), 5);
                order6.Confirm();
                orders.Add(order6);

                // Изчистваме domain events преди да запишем
                // (защото не искаме да публикуваме events за seed данни)
                foreach (var order in orders)
                {
                    order.ClearDomainEvents();
                }

                // Запис в базата
                await context.Orders.AddRangeAsync(orders);
                await context.SaveChangesAsync();

                logger.LogInformation(
                    "Успешно създадени {Count} тестови поръчки",
                    orders.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Грешка при seed на данни");
                throw;
            }
        }
    }
}