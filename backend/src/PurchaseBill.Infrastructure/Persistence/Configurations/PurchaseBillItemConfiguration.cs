using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurchaseBill.Application.Entities;

namespace PurchaseBill.Infrastructure.Persistence.Configurations;

public class PurchaseBillItemConfiguration : IEntityTypeConfiguration<PurchaseBillItem>
{
    public void Configure(EntityTypeBuilder<PurchaseBillItem> builder)
    {
        builder.ToTable("Purchase_Bill_Item");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ItemName).HasColumnName("Item_Name").HasMaxLength(100).IsRequired();
        // The dashboard donut groups all items by name.
        builder.HasIndex(i => i.ItemName);
        builder.Property(i => i.BatchLocationCode).HasColumnName("Batch_Location_Code").HasMaxLength(100).IsRequired();
        builder.Property(i => i.BatchLocationName).HasColumnName("Batch_Location_Name").HasMaxLength(200).IsRequired();

        builder.Property(i => i.StandardCost).HasColumnName("Standard_Cost").HasColumnType("decimal(18,2)");
        builder.Property(i => i.StandardPrice).HasColumnName("Standard_Price").HasColumnType("decimal(18,2)");
        builder.Property(i => i.Margin).HasColumnName("Margin").HasColumnType("decimal(18,2)");
        builder.Property(i => i.Quantity).HasColumnName("Quantity").HasColumnType("decimal(18,2)");
        builder.Property(i => i.FreeQuantity).HasColumnName("Free_Quantity").HasColumnType("decimal(18,2)");
        builder.Property(i => i.DiscountPercent).HasColumnName("Discount_Percent").HasColumnType("decimal(5,2)");
        builder.Property(i => i.TotalCost).HasColumnName("Total_Cost").HasColumnType("decimal(18,2)");
        builder.Property(i => i.TotalSelling).HasColumnName("Total_Selling").HasColumnType("decimal(18,2)");
    }
}
