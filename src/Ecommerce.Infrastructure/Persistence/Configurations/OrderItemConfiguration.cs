using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => oi.Id);
        builder.Property(oi => oi.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(oi => oi.Quantity).IsRequired();

        builder.OwnsOne(oi => oi.UnitPrice, price =>
        {
            price.Property(m => m.Amount).HasColumnName("UnitPrice").HasColumnType("decimal(18,2)");
            price.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        builder.HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
