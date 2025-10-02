using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.ValueObjetcs
{
    /// <summary>
    /// Value object за парична сума
    /// </summary>
    public class Money : ValueObject
    {
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }

        private Money() { Currency = "BGN"; } // За EF Core

        public Money(decimal amount, string currency = "BGN")
        {
            if (amount < 0)
            {
                throw new ArgumentException("Сумата не може да бъде отрицателна", nameof(amount));
            }

            if (string.IsNullOrWhiteSpace(currency))
            {
                throw new ArgumentException("Валутата е задължителна", nameof(currency));
            }

            Amount = amount;
            Currency = currency.ToUpperInvariant();
        }

        public Money Add(Money other)
        {
            if (Currency != other.Currency)
            {
                throw new InvalidOperationException("Не може да се събират суми в различни валути");
            }

            return new Money(Amount + other.Amount, Currency);
        }

        public Money Multiply(int quantity)
        {
            return new Money(Amount * quantity, Currency);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }

        public override string ToString() => $"{Amount:F2} {Currency}";
    }
}
