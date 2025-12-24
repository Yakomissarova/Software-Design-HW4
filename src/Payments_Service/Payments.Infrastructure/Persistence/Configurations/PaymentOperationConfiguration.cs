using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Entities.Models;

namespace Payments.Infrastructure.Persistence.Configurations;

public class PaymentOperationConfiguration : IEntityTypeConfiguration<PaymentOperation>
{
    public void Configure(EntityTypeBuilder<PaymentOperation> b)
    {
        b.ToTable("payment_operations");
        b.HasKey(x => x.Id);

        b.Property(x => x.OrderId).IsRequired();
        b.HasIndex(x => x.OrderId).IsUnique(); // гарантируем списывание денег ровно один раз

        b.Property(x => x.UserId).IsRequired();
        b.Property(x => x.Amount).IsRequired();
        b.Property(x => x.Status).IsRequired();
        b.Property(x => x.CreatedAt).IsRequired();
    }
}