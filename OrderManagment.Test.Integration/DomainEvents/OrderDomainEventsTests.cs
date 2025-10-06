using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Events;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Messaging;
using OrderManagement.Tests.Integration.Common;
using Shared.Contracts.Events;
using Xunit;

namespace OrderManagement.Tests.Integration.DomainEvents
{
    /// <summary>
    /// Integration tests за domain events и messaging
    /// Тестваме че domain events се dispatch-ват и integration events се публикуват
    /// </summary>
    public class OrderDomainEventsTests : IClassFixture<OrderManagementApiFactory>
    {
        private readonly OrderManagementApiFactory _factory;

        public OrderDomainEventsTests(OrderManagementApiFactory factory)
        {
            _factory = factory;
            _factory.ResetDatabase();
        }

        [Fact]
        public async Task SaveChanges_WithOrderCreated_ShouldDispatchOrderCreatedEvent()
        {
            // Arrange
            using var scope = _factory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();

            var order = Order.Create(
                Guid.NewGuid(),
                new Address("ул. Витоша 100", "София", "1000", "България"));

            order.AddItem(Guid.NewGuid(), "Test Product", new Money(100m), 1);

            // Act
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            // Assert
            // Domain event-ът трябва да е dispatch-нат и clear-нат
            order.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public async Task SaveChanges_WithOrderConfirmed_ShouldPublishIntegrationEvent()
        {
            // Arrange
            using var scope = _factory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
            var messageBusMock = scope.ServiceProvider.GetRequiredService<IMessageBus>();

            // Създаваме и потвърждаваме поръчка
            var order = Order.Create(
                Guid.NewGuid(),
                new Address("ул. Витоша 100", "София", "1000", "България"));

            order.AddItem(Guid.NewGuid(), "Test Product", new Money(100m), 2);
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            // Act
            order.Confirm();
            await db.SaveChangesAsync();

            // Assert
            // Verify че OrderConfirmedIntegrationEvent е публикуван към RabbitMQ
            var mock = Mock.Get(messageBusMock);
            mock.Verify(
                x => x.PublishAsync(
                    It.Is<OrderConfirmedIntegrationEvent>(e =>
                        e.OrderId == order.Id &&
                        e.CustomerId == order.CustomerId &&
                        e.TotalAmount == 200m),
                    "order.confirmed",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SaveChanges_WithOrderCancelled_ShouldPublishIntegrationEvent()
        {
            // Arrange
            using var scope = _factory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
            var messageBusMock = scope.ServiceProvider.GetRequiredService<IMessageBus>();

            var order = Order.Create(
                Guid.NewGuid(),
                new Address("ул. Витоша 100", "София", "1000", "България"));

            order.AddItem(Guid.NewGuid(), "Test Product", new Money(100m), 1);
            order.Confirm();
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            // Clear previous mock invocations
            var mock = Mock.Get(messageBusMock);
            mock.Invocations.Clear();

            // Act
            order.Cancel("Клиентът промени решението");
            await db.SaveChangesAsync();

            // Assert
            mock.Verify(
                x => x.PublishAsync(
                    It.Is<OrderCancelledIntegrationEvent>(e =>
                        e.OrderId == order.Id &&
                        e.CancellationReason == "Клиентът промени решението"),
                    "order.cancelled",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SaveChanges_WithOrderShipped_ShouldPublishIntegrationEvent()
        {
            // Arrange
            using var scope = _factory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
            var messageBusMock = scope.ServiceProvider.GetRequiredService<IMessageBus>();

            var order = Order.Create(
                Guid.NewGuid(),
                new Address("ул. Витоша 100", "София", "1000", "България"));

            order.AddItem(Guid.NewGuid(), "Test Product", new Money(100m), 1);
            order.Confirm();
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var mock = Mock.Get(messageBusMock);
            mock.Invocations.Clear();

            // Act
            order.MarkAsShipped();
            await db.SaveChangesAsync();

            // Assert
            mock.Verify(
                x => x.PublishAsync(
                    It.Is<OrderShippedIntegrationEvent>(e =>
                        e.OrderId == order.Id &&
                        e.ShippedDate != default),
                    "order.shipped",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SaveChanges_WithOrderDelivered_ShouldPublishIntegrationEvent()
        {
            // Arrange
            using var scope = _factory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
            var messageBusMock = scope.ServiceProvider.GetRequiredService<IMessageBus>();

            var order = Order.Create(
                Guid.NewGuid(),
                new Address("ул. Витоша 100", "София", "1000", "България"));

            order.AddItem(Guid.NewGuid(), "Test Product", new Money(100m), 1);
            order.Confirm();
            order.MarkAsShipped();
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var mock = Mock.Get(messageBusMock);
            mock.Invocations.Clear();

            // Act
            order.MarkAsDelivered();
            await db.SaveChangesAsync();

            // Assert
            mock.Verify(
                x => x.PublishAsync(
                    It.Is<OrderDeliveredIntegrationEvent>(e =>
                        e.OrderId == order.Id &&
                        e.DeliveredDate != default),
                    "order.delivered",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SaveChanges_WithMultipleEvents_ShouldDispatchAllEvents()
        {
            // Arrange
            using var scope = _factory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
            var messageBusMock = scope.ServiceProvider.GetRequiredService<IMessageBus>();

            var order1 = Order.Create(
                Guid.NewGuid(),
                new Address("ул. Витоша 100", "София", "1000", "България"));
            order1.AddItem(Guid.NewGuid(), "Product 1", new Money(100m), 1);

            var order2 = Order.Create(
                Guid.NewGuid(),
                new Address("бул. Витоша 200", "София", "1000", "България"));
            order2.AddItem(Guid.NewGuid(), "Product 2", new Money(200m), 1);

            db.Orders.Add(order1);
            db.Orders.Add(order2);

            // Act
            await db.SaveChangesAsync();

            // Assert
            // Двата Orders са създадени, така че трябва да имаме 2 OrderCreatedEvent-а
            order1.DomainEvents.Should().BeEmpty(); // Clear-нати след dispatch
            order2.DomainEvents.Should().BeEmpty();
        }
    }
}