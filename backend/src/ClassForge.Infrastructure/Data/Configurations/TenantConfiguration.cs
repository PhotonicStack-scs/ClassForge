using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassForge.Infrastructure.Data.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.DefaultLanguage).IsRequired().HasMaxLength(10).HasDefaultValue("nb");
        builder.Property(t => t.SetupCompleted).IsRequired().HasDefaultValue(false);
        builder.Property(t => t.SetupProgressJson).HasColumnType("text");
    }
}
