using FluentAssertions;
using OrderManagement.Application.Commands.CreateOrder;
using Xunit;

namespace OrderManagement.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests за CreateOrderCommandValidator
    /// Тестваме всички validation правила
    /// </summary>
    public class CreateOrderCommandValidatorTests
    {
        private readonly CreateOrderCommandValidator _validator;

        public CreateOrderCommandValidatorTests()
        {
            _validator = new CreateOrderCommandValidator();
        }

        [Fact]
        public void Validate_WithValidCommand_ShouldPass()
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

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithEmptyCustomerId_ShouldFail()
        {
            // Arrange
            var command = new CreateOrderCommand
            {
                CustomerId = Guid.Empty,
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

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.ErrorMessage.Should().Contain("ID на клиента е задължително");
        }

        [Fact]
        public void Validate_WithNullAddress_ShouldFail()
        {
            // Arrange
            var command = new CreateOrderCommand
            {
                CustomerId = Guid.NewGuid(),
                ShippingAddress = null!,
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

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("Адресът за доставка е задължителен"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_WithInvalidStreet_ShouldFail(string street)
        {
            // Arrange
            var command = new CreateOrderCommand
            {
                CustomerId = Guid.NewGuid(),
                ShippingAddress = new CreateOrderCommand.AddressDto
                {
                    Street = street,
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

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("Улицата е задължителна"));
        }

        [Fact]
        public void Validate_WithEmptyItems_ShouldFail()
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
                Items = new List<CreateOrderCommand.OrderItemDto>()
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.ErrorMessage.Should().Contain("поне един продукт");
        }

        [Fact]
        public void Validate_WithInvalidItemPrice_ShouldFail()
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
                        UnitPrice = 0m, // Invalid!
                        Quantity = 1
                    }
                }
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("Цената трябва да е положителна"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidQuantity_ShouldFail(int quantity)
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
                        Quantity = quantity
                    }
                }
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("Количеството трябва да е положително число"));
        }

        [Fact]
        public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
        {
            // Arrange
            var command = new CreateOrderCommand
            {
                CustomerId = Guid.Empty, // Error 1
                ShippingAddress = new CreateOrderCommand.AddressDto
                {
                    Street = "", // Error 2
                    City = "", // Error 3
                    PostalCode = "1000",
                    Country = "България"
                },
                Items = new List<CreateOrderCommand.OrderItemDto>() // Error 4
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCountGreaterThan(3);
        }
    }
}