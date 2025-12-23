using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Entities.Models;

namespace Payments.Infrastructure.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> b)
    {
        b.ToTable("outbox_messages");
        b.HasKey(x => x.Id);

        b.Property(x => x.Type).IsRequired();
        b.Property(x => x.Payload).IsRequired();
        b.Property(x => x.Status).IsRequired();
        b.Property(x => x.Attempts).IsRequired();
        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.ProcessedAt);

        b.HasIndex(x => x.Status); // быстро выбирать Pending
    }
}