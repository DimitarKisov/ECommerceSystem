using FluentAssertions;
using OrderManagement.Domain.ValueObjects;
using Xunit;

namespace OrderManagement.Tests.Unit.Domain
{
    /// <summary>
    /// Unit tests за Value Objects
    /// Тестваме equality, validation и domain операции
    /// </summary>
    public class MoneyTests
    {
        [Fact]
        public void Create_WithValidAmount_ShouldCreateMoney()
        {
            // Act
            var money = new Money(100.50m, "BGN");

            // Assert
            money.Amount.Should().Be(100.50m);
            money.Currency.Should().Be("BGN");
        }

        [Fact]
        public void Create_WithNegativeAmount_ShouldThrowException()
        {
            // Act
            var act = () => new Money(-10m);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*не може да бъде отрицателна*");
        }

        [Fact]
        public void Add_WithSameCurrency_ShouldReturnSum()
        {
            // Arrange
            var money1 = new Money(100m, "BGN");
            var money2 = new Money(50m, "BGN");

            // Act
            var result = money1.Add(money2);

            // Assert
            result.Amount.Should().Be(150m);
            result.Currency.Should().Be("BGN");
        }

        [Fact]
        public void Add_WithDifferentCurrency_ShouldThrowException()
        {
            // Arrange
            var money1 = new Money(100m, "BGN");
            var money2 = new Money(50m, "EUR");

            // Act
            var act = () => money1.Add(money2);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*не може да се събират суми в различни валути*");
        }

        [Fact]
        public void Multiply_ShouldMultiplyAmount()
        {
            // Arrange
            var money = new Money(10m, "BGN");

            // Act
            var result = money.Multiply(5);

            // Assert
            result.Amount.Should().Be(50m);
            result.Currency.Should().Be("BGN");
        }

        [Fact]
        public void Equality_WithSameValues_ShouldBeEqual()
        {
            // Arrange
            var money1 = new Money(100m, "BGN");
            var money2 = new Money(100m, "BGN");

            // Act & Assert
            money1.Should().Be(money2);
            (money1 == money2).Should().BeTrue();
        }

        [Fact]
        public void Equality_WithDifferentValues_ShouldNotBeEqual()
        {
            // Arrange
            var money1 = new Money(100m, "BGN");
            var money2 = new Money(100m, "EUR");

            // Act & Assert
            money1.Should().NotBe(money2);
            (money1 != money2).Should().BeTrue();
        }

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            var money = new Money(1234.56m, "BGN");

            // Act
            var result = money.ToString();

            // Assert
            result.Should().Be("1234.56 BGN");
        }
    }

    public class AddressTests
    {
        [Fact]
        public void Create_WithValidData_ShouldCreateAddress()
        {
            // Act
            var address = new Address(
                "ул. Витоша 100",
                "София",
                "1000",
                "България");

            // Assert
            address.Street.Should().Be("ул. Витоша 100");
            address.City.Should().Be("София");
            address.PostalCode.Should().Be("1000");
            address.Country.Should().Be("България");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithInvalidStreet_ShouldThrowException(string street)
        {
            // Act
            var act = () => new Address(street, "София", "1000", "България");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*улицата е задължителна*");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithInvalidCity_ShouldThrowException(string city)
        {
            // Act
            var act = () => new Address("ул. Витоша 100", city, "1000", "България");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*градът е задължителен*");
        }

        [Fact]
        public void Equality_WithSameValues_ShouldBeEqual()
        {
            // Arrange
            var address1 = new Address("ул. Витоша 100", "София", "1000", "България");
            var address2 = new Address("ул. Витоша 100", "София", "1000", "България");

            // Act & Assert
            address1.Should().Be(address2);
            (address1 == address2).Should().BeTrue();
        }

        [Fact]
        public void Equality_WithDifferentValues_ShouldNotBeEqual()
        {
            // Arrange
            var address1 = new Address("ул. Витоша 100", "София", "1000", "България");
            var address2 = new Address("бул. Витоша 100", "София", "1000", "България");

            // Act & Assert
            address1.Should().NotBe(address2);
            (address1 != address2).Should().BeTrue();
        }

        [Fact]
        public void ToString_ShouldReturnFormattedAddress()
        {
            // Arrange
            var address = new Address("ул. Витоша 100", "София", "1000", "България");

            // Act
            var result = address.ToString();

            // Assert
            result.Should().Be("ул. Витоша 100, София 1000, България");
        }
    }
}