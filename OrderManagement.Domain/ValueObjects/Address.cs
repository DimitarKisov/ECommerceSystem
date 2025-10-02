using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.ValueObjetcs
{
    /// <summary>
    /// Value object за адрес
    /// </summary>
    public class Address : ValueObject
    {
        public string Street { get; private set; }
        public string City { get; private set; }
        public string PostalCode { get; private set; }
        public string Country { get; private set; }

        private Address()
        {
            Street = string.Empty;
            City = string.Empty;
            PostalCode = string.Empty;
            Country = string.Empty;
        } // За EF Core

        public Address(string street, string city, string postalCode, string country)
        {
            if (string.IsNullOrWhiteSpace(street))
            {
                throw new ArgumentException("Улицата е задължителна", nameof(street));
            }

            if (string.IsNullOrWhiteSpace(city))
            {
                throw new ArgumentException("Градът е задължителен", nameof(city));
            }

            Street = street;
            City = city;
            PostalCode = postalCode;
            Country = country;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Street;
            yield return City;
            yield return PostalCode;
            yield return Country;
        }

        public override string ToString() => $"{Street}, {City} {PostalCode}, {Country}";
    }
}
