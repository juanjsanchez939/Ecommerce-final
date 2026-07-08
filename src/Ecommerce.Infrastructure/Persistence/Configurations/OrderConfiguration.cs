using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(50);

        builder.OwnsOne(o => o.ShippingAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("ShippingStreet").HasMaxLength(200);
            address.Property(a => a.City).HasColumnName("ShippingCity").HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("ShippingState").HasMaxLength(100);
            address.Property(a => a.Country).HasColumnName("ShippingCountry").HasMaxLength(100);
            address.Property(a => a.ZipCode).HasColumnName("ShippingZipCode").HasMaxLength(20);
        });

        builder.HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
