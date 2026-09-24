using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseBill.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPoNumberAndDashboardIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Po_Number",
                table: "Purchase_Bill",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            // Backfill bills saved before this migration so the unique index below can be created.
            migrationBuilder.Sql(
                "UPDATE [Purchase_Bill] SET [Po_Number] = 'PO-' + RIGHT('000000' + CAST([Id] AS varchar(10)), 6)");

            migrationBuilder.CreateIndex(
                name: "IX_Purchase_Bill_Po_Number",
                table: "Purchase_Bill",
                column: "Po_Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchase_Bill_Created_At",
                table: "Purchase_Bill",
                column: "Created_At");

            migrationBuilder.CreateIndex(
                name: "IX_Purchase_Bill_Item_Item_Name",
                table: "Purchase_Bill_Item",
                column: "Item_Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Purchase_Bill_Item_Item_Name", table: "Purchase_Bill_Item");
            migrationBuilder.DropIndex(name: "IX_Purchase_Bill_Created_At", table: "Purchase_Bill");
            migrationBuilder.DropIndex(name: "IX_Purchase_Bill_Po_Number", table: "Purchase_Bill");
            migrationBuilder.DropColumn(name: "Po_Number", table: "Purchase_Bill");
        }
    }
}
