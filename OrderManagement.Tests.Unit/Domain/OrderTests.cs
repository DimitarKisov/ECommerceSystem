using FluentAssertions;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Events;
using OrderManagement.Domain.ValueObjects;
using Xunit;

namespace OrderManagement.Tests.Unit.Domain
{
    /// <summary>
    /// Unit tests за Order entity
    /// Тестваме domain логиката и бизнес правилата
    /// </summary>
    public class OrderTests
    {
        private readonly Guid _customerId = Guid.NewGuid();
        private readonly Address _shippingAddress = new("ул. Витоша 100", "София", "1000", "България");

        [Fact]
        public void Create_ShouldCreateOrderWithPendingStatus()
        {
            // Act
            var order = Order.Create(_customerId, _shippingAddress);

            // Assert
            order.Should().NotBeNull();
            order.Id.Should().NotBeEmpty();
            order.OrderNumber.Should().NotBeNullOrEmpty();
            order.CustomerId.Should().Be(_customerId);
            order.Status.Should().Be(OrderStatus.Pending);
            order.TotalAmount.Amount.Should().Be(0);
            order.Items.Should().BeEmpty();
            order.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<OrderCreatedEvent>();
        }

        [Fact]
        public void AddItem_ShouldAddItemToOrder()
        {
            // Arrange
            var order = Order.Create(_customerId, _shippingAddress);
            var productId = Guid.NewGuid();
            var unitPrice = new Money(100m);

            // Act
            order.AddItem(productId, "Test Product", unitPrice, 2);

            // Assert
            order.Items.Should().ContainSingle();
            var item = order.Items.First();
            item.ProductId.Should().Be(productId);
            item.Quantity.Should().Be(2);
            order.TotalAmount.Amount.Should().Be(200m);
        }

        [Fact]
        public void AddItem_WhenProductExists_ShouldIncreaseQuantity()
        {
            // Arrange
            var order = Order.Create(_customerId, _shippingAddress);
            var productId = Guid.NewGuid();
            var unitPrice = new Money(100m);

            // Act
            order.AddItem(productId, "Test Product", unitPrice, 2);
            order.AddItem(productId, "Test Product", unitPrice, 3);

            // Assert
            order.Items.Should().ContainSingle();
            order.Items.First().Quantity.Should().Be(5);
            order.TotalAmount.Amount.Should().Be(500m);
        }

        [Fact]
        public void AddItem_WhenOrderConfirmed_ShouldThrowException()
        {
            // Arrange
            var order = Order.Create(_customerId, _shippingAddress);
            order.AddItem(Guid.NewGuid(), "Test Product", new Money(100m), 1);
            order.Confirm();

            // Act
            var act = () => order.AddItem(Guid.NewGuid(), "Another Product", new Money(50m), 1);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*не може да се добавят продукти към потвърдена поръчка*");
        }

        [Fact]
        public void RemoveItem_ShouldRemoveItemAndRecalculateTotal()
        {
            // Arrange
            var order = Order.Create(_customerId, _shippingAddress);
            var productId = Guid.NewGuid();
            order.AddItem(productId, "Test Product", new Money(100m), 2);
            order.AddItem(Guid.NewGuid(), "Another Product", new Money(50m), 1);

            // Act
            order.RemoveItem(productId);

            // Assert
            order.Items.Should().ContainSingle();
            order.TotalAmount.Amount.Should().Be(50m);
        }

        [Fact]
        public void Confirm_WithItems_ShouldConfirmOrderAndRaiseDomainEvent()
        {
            // Arrange
            var order = Order.Create(_customerId, _shippingAddress);
            order.AddItem(Guid.NewGuid(), "Test Product", new Money(100m), 2);
            order.ClearDomainEvents(); // Изчистваме OrderCreatedEvent

            // Act
            order.Confirm();

            // Assert
            order.Status.Should().Be(OrderStatus.Confirmed);
            order.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<OrderConfirmedEvent>();
        }

        [Fact]
        public void Confirm_WithoutItems_ShouldThrowException()
        {
            // Arrange
            var order = Order.Create(_customerId, _shippingAddress);

            // Act
            var act = () => order.Confirm();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*не може да се потвърди празна поръчка*");
        }

        [Fact]
        public void Confirm_WhenAlreadyConfirmed_ShouldThrowException()
        {
            // Arrange
            var order = Order.Create(_customerId, _shippingAddress);
            order.AddItem(Guid.NewGuid(), "Test Product", new Money(100m), 1);
            order.Confirm();

            // Act
            var act = () => order.Confirm();

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Cancel_WithValidReason_ShouldCancelOrderAndRaiseDomainEvent()
        {
            // Arrange
            var order = Order.Create(_customerId, _shippingAddress);
            order.AddItem(Guid.NewGuid(), "Test Product", new Money(100m), 1);
            order.Confirm();
            order.ClearDomainEvents();

            // Act
            order.Cancel("Клиентът промени решението");

            // Assert
            order.Status.Should().Be(OrderStatus.Cancelled);
            order.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<OrderCancelledEvent>();
        }

        [Fact]
        public void Cancel_WhenDelivered_ShouldThrowException()
        {
            // Arrange
            var order = Order.Create(_customerId, _shippingAddress);
            order.AddItem(Guid.NewGuid(), "Test Product", new Money(100m), 1);
            order.Confirm();
            order.MarkAsShipped();
            order.MarkAsDelivered();

            // Act
            var act = () => order.Cancel("Test reason");

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void MarkAsShipped_WhenConfirmed_ShouldUpdateStatusAndRaiseDomainEvent()
        {
            // Arrange
            var order = Order.Create(_customerId, _shippingAddress);
            order.AddItem(Guid.NewGuid(), "Test Product", new Money(100m), 1);
            order.Confirm();
            order.ClearDomainEvents();

            // Act
            order.MarkAsShipped();

            // Assert
            order.Status.Should().Be(OrderStatus.Shipped);
            order.ShippedDate.Should().NotBeNull();
            order.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<OrderShippedEvent>();
        }

        [Fact]
        public void MarkAsDelivered_WhenShipped_ShouldUpdateStatusAndRaiseDomainEvent()
        {
            // Arrange
            var order = Order.Create(_customerId, _shippingAddress);
            order.AddItem(Guid.NewGuid(), "Test Product", new Money(100m), 1);
            order.Confirm();
            order.MarkAsShipped();
            order.ClearDomainEvents();

            // Act
            order.MarkAsDelivered();

            // Assert
            order.Status.Should().Be(OrderStatus.Delivered);
            order.DeliveredDate.Should().NotBeNull();
            order.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<OrderDeliveredEvent>();
        }

        [Fact]
        public void MarkAsDelivered_WhenNotShipped_ShouldThrowException()
        {
            // Arrange
            var order = Order.Create(_customerId, _shippingAddress);
            order.AddItem(Guid.NewGuid(), "Test Product", new Money(100m), 1);
            order.Confirm();

            // Act
            var act = () => order.MarkAsDelivered();

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void AddItem_WithInvalidQuantity_ShouldThrowException(int quantity)
        {
            // Arrange
            var order = Order.Create(_customerId, _shippingAddress);

            // Act
            var act = () => order.AddItem(
                Guid.NewGuid(),
                "Test Product",
                new Money(100m),
                quantity);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*количеството трябва да е положително число*");
        }
    }
}