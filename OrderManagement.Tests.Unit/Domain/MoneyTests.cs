using FluentAssertions;
using OrderManagement.Domain.ValueObjects;
using System.Globalization;
using Xunit;

namespace OrderManagement.Tests.Unit.Domain
{
    /// <summary>
    /// Unit tests за Money Value Object
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
        public void ToString_ShouldReturnFormattedString_UsingInvariantCulture()
        {
            // Arrange
            var money = new Money(1234.56m, "BGN");

            // Act
            var result = money.ToString();

            // Assert
            // Ако използваме InvariantCulture в Money.ToString(), очакваме точка
            result.Should().Be("1234.56 BGN");
        }

        [Fact]
        public void ToString_ShouldBeConsistent_AcrossDifferentCultures()
        {
            // Arrange
            var money = new Money(1234.56m, "EUR");
            var originalCulture = CultureInfo.CurrentCulture;

            try
            {
                // Act - тестваме с различни cultures
                CultureInfo.CurrentCulture = new CultureInfo("bg-BG"); // Българска
                var resultBg = money.ToString();

                CultureInfo.CurrentCulture = new CultureInfo("en-US"); // Американска
                var resultEn = money.ToString();

                CultureInfo.CurrentCulture = new CultureInfo("de-DE"); // Немска
                var resultDe = money.ToString();

                // Assert - всички трябва да са еднакви (InvariantCulture)
                resultBg.Should().Be("1234.56 EUR");
                resultEn.Should().Be("1234.56 EUR");
                resultDe.Should().Be("1234.56 EUR");
                resultBg.Should().Be(resultEn);
                resultEn.Should().Be(resultDe);
            }
            finally
            {
                // Възстановяваме оригиналната culture
                CultureInfo.CurrentCulture = originalCulture;
            }
        }
    }
}