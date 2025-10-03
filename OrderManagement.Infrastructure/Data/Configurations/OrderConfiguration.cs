using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Infrastructure.Data.Configurations
{
    /// <summary>
    /// EF Core конфигурация за Order entity
    /// </summary>
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(o => o.OrderNumber)
                .IsUnique();

            builder.Property(o => o.CustomerId)
                .IsRequired();

            builder.Property(o => o.OrderDate)
                .IsRequired();

            builder.Property(o => o.Status)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => (OrderStatus)Enum.Parse(typeof(OrderStatus), v))
                .HasMaxLength(20);

            // Конфигурация за Address value object (owned entity)
            builder.OwnsOne(o => o.ShippingAddress, address =>
            {
                address.Property(a => a.Street)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnName("ShippingStreet");

                address.Property(a => a.City)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("ShippingCity");

                address.Property(a => a.PostalCode)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("ShippingPostalCode");

                address.Property(a => a.Country)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("ShippingCountry");
            });

            // Конфигурация за Money value object (owned entity)
            builder.OwnsOne(o => o.TotalAmount, money =>
            {
                money.Property(m => m.Amount)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)")
                    .HasColumnName("TotalAmount");

                money.Property(m => m.Currency)
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasColumnName("Currency");
            });

            builder.Property(o => o.ShippedDate)
                .IsRequired(false);

            builder.Property(o => o.DeliveredDate)
                .IsRequired(false);

            builder.Property(o => o.CancellationReason)
                .HasMaxLength(500)
                .IsRequired(false);

            // Връзка с OrderItems
            builder.HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Игнорираме DomainEvents колекцията
            builder.Ignore(o => o.DomainEvents);
        }
    }
}