using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Data.Configurations
{
    /// <summary>
    /// EF Core конфигурация за OrderItem entity
    /// </summary>
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");

            builder.HasKey(oi => oi.Id);

            builder.Property(oi => oi.OrderId)
                .IsRequired();

            builder.Property(oi => oi.ProductId)
                .IsRequired();

            builder.Property(oi => oi.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(oi => oi.Quantity)
                .IsRequired();

            // Конфигурация за UnitPrice (Money value object)
            builder.OwnsOne(oi => oi.UnitPrice, money =>
            {
                money.Property(m => m.Amount)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)")
                    .HasColumnName("UnitPrice");

                money.Property(m => m.Currency)
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasColumnName("UnitPriceCurrency");
            });

            // Конфигурация за Subtotal (Money value object)
            builder.OwnsOne(oi => oi.Subtotal, money =>
            {
                money.Property(m => m.Amount)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)")
                    .HasColumnName("Subtotal");

                money.Property(m => m.Currency)
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasColumnName("SubtotalCurrency");
            });

            // Индекс за бързо търсене по ProductId
            builder.HasIndex(oi => oi.ProductId);

            // Игнорираме DomainEvents
            builder.Ignore(oi => oi.DomainEvents);
        }
    }
}