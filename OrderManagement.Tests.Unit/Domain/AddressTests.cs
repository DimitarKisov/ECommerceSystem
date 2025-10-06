using FluentAssertions;
using OrderManagement.Domain.ValueObjects;
using Xunit;

namespace OrderManagement.Tests.Unit.Domain
{
    /// <summary>
    /// Unit tests за Address Value Object
    /// </summary>
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

        [Fact]
        public void GetHashCode_WithSameValues_ShouldReturnSameHashCode()
        {
            // Arrange
            var address1 = new Address("ул. Витоша 100", "София", "1000", "България");
            var address2 = new Address("ул. Витоша 100", "София", "1000", "България");

            // Act & Assert
            address1.GetHashCode().Should().Be(address2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_WithDifferentValues_ShouldReturnDifferentHashCode()
        {
            // Arrange
            var address1 = new Address("ул. Витоша 100", "София", "1000", "България");
            var address2 = new Address("ул. Витоша 200", "София", "1000", "България");

            // Act & Assert
            address1.GetHashCode().Should().NotBe(address2.GetHashCode());
        }
    }
}