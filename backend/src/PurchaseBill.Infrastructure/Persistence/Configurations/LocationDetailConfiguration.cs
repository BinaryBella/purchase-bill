using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurchaseBill.Application.Entities;

namespace PurchaseBill.Infrastructure.Persistence.Configurations;

public class LocationDetailConfiguration : IEntityTypeConfiguration<LocationDetail>
{
    public void Configure(EntityTypeBuilder<LocationDetail> builder)
    {
        builder.ToTable("Location_Details");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.LocationCode)
            .HasColumnName("Location_Code")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(l => l.LocationName)
            .HasColumnName("Location_Name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(l => l.CreatedAt).HasColumnName("Created_At");
        builder.Property(l => l.UpdatedAt).HasColumnName("Updated_At");

        builder.HasIndex(l => l.LocationCode).IsUnique();
    }
}
