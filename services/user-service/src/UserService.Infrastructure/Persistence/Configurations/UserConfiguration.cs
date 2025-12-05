using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");
            builder.HasKey(u => u.UserId);

            // String properties with max length
            builder.Property(u => u.Email)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(u => u.FirstName)
                   .HasMaxLength(100);

            builder.Property(u => u.LastName)
                   .HasMaxLength(100);

            builder.Property(u => u.KeycloakId)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(u => u.PhoneNumber)
                   .HasMaxLength(20);

            // Indexes
            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.KeycloakId).IsUnique();

            // Relationships
            builder.HasMany(u => u.Addresses)
                   .WithOne(a => a.User)
                   .HasForeignKey(a => a.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.Preference)
                   .WithOne(p => p.User)
                   .HasForeignKey<UserService.Domain.Entities.UserPreference>(p => p.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.Statistics)
                   .WithOne(s => s.User)
                   .HasForeignKey<UserService.Domain.Entities.UserStatistics>(s => s.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
