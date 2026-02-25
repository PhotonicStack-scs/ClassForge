using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassForge.Infrastructure.Data.Configurations;

public class TimetableEntryClassConfiguration : IEntityTypeConfiguration<TimetableEntryClass>
{
    public void Configure(EntityTypeBuilder<TimetableEntryClass> builder)
    {
        builder.HasKey(ec => new { ec.TimetableEntryId, ec.ClassId });
        builder.HasOne(ec => ec.TimetableEntry).WithMany(e => e.Classes).HasForeignKey(ec => ec.TimetableEntryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(ec => ec.Class).WithMany().HasForeignKey(ec => ec.ClassId).OnDelete(DeleteBehavior.NoAction);
    }
}
