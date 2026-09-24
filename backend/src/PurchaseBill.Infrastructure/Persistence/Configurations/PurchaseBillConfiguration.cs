using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurchaseBillEntity = PurchaseBill.Application.Entities.PurchaseBill;

namespace PurchaseBill.Infrastructure.Persistence.Configurations;

public class PurchaseBillConfiguration : IEntityTypeConfiguration<PurchaseBillEntity>
{
    public void Configure(EntityTypeBuilder<PurchaseBillEntity> builder)
    {
        builder.ToTable("Purchase_Bill");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.PoNumber).HasColumnName("Po_Number").HasMaxLength(40).IsRequired();
        builder.HasIndex(b => b.PoNumber).IsUnique();

        builder.Property(b => b.CreatedByUsername).HasColumnName("Created_By_Username").HasMaxLength(256).IsRequired();
        builder.Property(b => b.CreatedAt).HasColumnName("Created_At");
        // Dashboard widgets sort and filter bills by date.
        builder.HasIndex(b => b.CreatedAt);

        builder.Property(b => b.TotalItems).HasColumnName("Total_Items");
        builder.Property(b => b.TotalQuantity).HasColumnName("Total_Quantity").HasColumnType("decimal(18,2)");
        builder.Property(b => b.TotalCost).HasColumnName("Total_Cost").HasColumnType("decimal(18,2)");
        builder.Property(b => b.TotalSelling).HasColumnName("Total_Selling").HasColumnType("decimal(18,2)");

        builder.HasMany(b => b.Items)
            .WithOne(i => i.PurchaseBill)
            .HasForeignKey(i => i.PurchaseBillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
