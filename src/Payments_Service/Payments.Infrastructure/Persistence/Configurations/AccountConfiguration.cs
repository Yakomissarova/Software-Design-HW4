using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Entities.Models;

namespace Payments.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> b)
    {
        b.ToTable("accounts");
        b.HasKey(x => x.UserId);

        b.Property(x => x.Login).IsRequired();
        b.HasIndex(x => x.Login).IsUnique(); // логин должен быть уникален

        b.Property(x => x.Balance).IsRequired();
        // Для SQLite decimal нормально хранится как NUMERIC/REAL; EF подберет тип.
        // Если хотите жестко: b.Property(x => x.Balance).HasColumnType("NUMERIC");
    }
}