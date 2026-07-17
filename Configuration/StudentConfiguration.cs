using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

namespace TmsApi.Configuration;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.RegistrationNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(s => s.RegistrationNumber)
            .IsUnique();

        builder.Property(s => s.GPA)
            .IsRequired();

        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);

        builder.Property(s => s.IsDeleted)
            .HasDefaultValue(false);

        // Shadow property for audit
        builder.Property<DateTime>("LastUpdated");

        // PostgreSQL concurrency token (uses xmin)
        builder.Property(s => s.Version)
               .IsRowVersion();

        // Soft delete filter
      
    }
}