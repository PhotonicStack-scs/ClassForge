using ClassForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassForge.Infrastructure.Data.Configurations;

public class TimetableEntryGroupConfiguration : IEntityTypeConfiguration<TimetableEntryGroup>
{
    public void Configure(EntityTypeBuilder<TimetableEntryGroup> builder)
    {
        builder.HasKey(eg => new { eg.TimetableEntryId, eg.GroupId });
        builder.HasOne(eg => eg.TimetableEntry).WithMany(e => e.Groups).HasForeignKey(eg => eg.TimetableEntryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(eg => eg.Group).WithMany().HasForeignKey(eg => eg.GroupId).OnDelete(DeleteBehavior.NoAction);
    }
}
