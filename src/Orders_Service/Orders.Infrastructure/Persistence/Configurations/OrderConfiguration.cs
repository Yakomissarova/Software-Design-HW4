using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Entities.Models;

namespace Orders.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("orders");
        b.HasKey(x => x.Id);

        b.Property(x => x.PublicId).IsRequired();
        b.HasIndex(x => x.PublicId).IsUnique();

        b.Property(x => x.UserId).IsRequired();
        b.Property(x => x.Amount).IsRequired();
        b.Property(x => x.Description).IsRequired();
        b.Property(x => x.Status).IsRequired();
        b.Property(x => x.CreatedAt).IsRequired();
    }
}