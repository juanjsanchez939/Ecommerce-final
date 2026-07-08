using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Persistence.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(ci => ci.Id);
        builder.Property(ci => ci.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(ci => ci.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(ci => ci.Currency).HasMaxLength(3).IsRequired();
        builder.Property(ci => ci.Quantity).IsRequired();
    }
}
