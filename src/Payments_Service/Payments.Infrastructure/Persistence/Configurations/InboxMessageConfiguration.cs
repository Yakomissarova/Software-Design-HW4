using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Entities.Models;

namespace Payments.Infrastructure.Persistence.Configurations;

public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> b)
    {
        b.ToTable("inbox_messages");
        b.HasKey(x => x.MessageId); // уникальность входящего сообщения

        b.Property(x => x.Type).IsRequired();
        b.Property(x => x.Payload).IsRequired();
        b.Property(x => x.ReceivedAt).IsRequired();
        b.Property(x => x.ProcessedAt);
    }
}