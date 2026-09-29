using LiteFactoryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LiteFactoryApi.Data;

public sealed class LiteFactoryDbContext(DbContextOptions<LiteFactoryDbContext> options) : DbContext(options)
{
    public DbSet<LiteFactoryUser> Users => Set<LiteFactoryUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<LiteFactoryUser>();
        user.HasKey(value => value.Id);
        user.Property(value => value.Email).HasMaxLength(254).IsRequired();
        user.Property(value => value.NormalizedEmail).HasMaxLength(254).IsRequired();
        user.Property(value => value.Nickname).HasMaxLength(20).IsRequired();
        user.Property(value => value.NormalizedNickname).HasMaxLength(20).IsRequired();
        user.Property(value => value.PasswordHash).IsRequired();
        user.Property(value => value.CreatedAtUtc).IsRequired();
        user.Property(value => value.IsActive).IsRequired();
        user.HasIndex(value => value.NormalizedEmail).IsUnique();
        user.HasIndex(value => value.NormalizedNickname).IsUnique();
    }
}
