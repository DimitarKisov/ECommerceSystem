using FluentAssertions;
using Moq;
using OrderManagement.Application.Commands.CreateOrder;
using OrderManagement.Application.Common;
using OrderManagement.Domain.Entities;
using Xunit;

namespace OrderManagement.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests за CreateOrderCommandHandler
    /// Използваме Moq за mocking на IOrderService
    /// </summary>
    public class CreateOrderCommandHandlerTests
    {
        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly CreateOrderCommand.CreateOrderCommandHandler _handler;

        public CreateOrderCommandHandlerTests()
        {
            _orderServiceMock = new Mock<IOrderService>();
            _handler = new CreateOrderCommand.CreateOrderCommandHandler(_orderServiceMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidCommand_ShouldCreateOrderSuccessfully()
        {
            // Arrange
            var command = new CreateOrderCommand
            {
                CustomerId = Guid.NewGuid(),
                ShippingAddress = new CreateOrderCommand.AddressDto
                {
                    Street = "ул. Витоша 100",
                    City = "София",
                    PostalCode = "1000",
                    Country = "България"
                },
                Items = new List<CreateOrderCommand.OrderItemDto>
                {
                    new()
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Test Product",
                        UnitPrice = 100m,
                        Quantity = 2
                    }
                }
            };

            // Setup mock за да не throw-не exception
            _orderServiceMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeEmpty();

            // Verify че Add е извикан веднъж
            _orderServiceMock.Verify(
                x => x.Add(It.Is<Order>(o =>
                    o.CustomerId == command.CustomerId &&
                    o.Items.Count == 1)),
                Times.Once);

            // Verify че SaveChangesAsync е извикан веднъж
            _orderServiceMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenSaveChangesFails_ShouldReturnFailure()
        {
            // Arrange
            var command = new CreateOrderCommand
            {
                CustomerId = Guid.NewGuid(),
                ShippingAddress = new CreateOrderCommand.AddressDto
                {
                    Street = "ул. Витоша 100",
                    City = "София",
                    PostalCode = "1000",
                    Country = "България"
                },
                Items = new List<CreateOrderCommand.OrderItemDto>
                {
                    new()
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Test Product",
                        UnitPrice = 100m,
                        Quantity = 1
                    }
                }
            };

            // Setup mock да throw exception
            _orderServiceMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Грешка при създаване на поръчка");
            result.Error.Should().Contain("Database error");
        }

        [Fact]
        public async Task Handle_WithMultipleItems_ShouldAddAllItems()
        {
            // Arrange
            var command = new CreateOrderCommand
            {
                CustomerId = Guid.NewGuid(),
                ShippingAddress = new CreateOrderCommand.AddressDto
                {
                    Street = "ул. Витоша 100",
                    City = "София",
                    PostalCode = "1000",
                    Country = "България"
                },
                Items = new List<CreateOrderCommand.OrderItemDto>
                {
                    new()
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Product 1",
                        UnitPrice = 100m,
                        Quantity = 1
                    },
                    new()
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Product 2",
                        UnitPrice = 200m,
                        Quantity = 2
                    }
                }
            };

            _orderServiceMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            _orderServiceMock.Verify(
                x => x.Add(It.Is<Order>(o => o.Items.Count == 2)),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCalculateTotalAmountCorrectly()
        {
            // Arrange
            var command = new CreateOrderCommand
            {
                CustomerId = Guid.NewGuid(),
                ShippingAddress = new CreateOrderCommand.AddressDto
                {
                    Street = "ул. Витоша 100",
                    City = "София",
                    PostalCode = "1000",
                    Country = "България"
                },
                Items = new List<CreateOrderCommand.OrderItemDto>
                {
                    new()
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Product 1",
                        UnitPrice = 100m,
                        Quantity = 2 // 200
                    },
                    new()
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Product 2",
                        UnitPrice = 50m,
                        Quantity = 3 // 150
                    }
                }
            };

            _orderServiceMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            _orderServiceMock.Verify(
                x => x.Add(It.Is<Order>(o => o.TotalAmount.Amount == 350m)),
                Times.Once);
        }
    }
}