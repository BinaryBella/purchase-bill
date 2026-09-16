using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseBill.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Location_Details",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Location_Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Location_Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Created_At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated_At = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location_Details", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Purchase_Bill",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Created_By_Username = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Created_At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Total_Items = table.Column<int>(type: "int", nullable: false),
                    Total_Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total_Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total_Selling = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchase_Bill", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Purchase_Bill_Item",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseBillId = table.Column<int>(type: "int", nullable: false),
                    Item_Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Batch_Location_Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Batch_Location_Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Standard_Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Standard_Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Margin = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Free_Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount_Percent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Total_Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total_Selling = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchase_Bill_Item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Purchase_Bill_Item_Purchase_Bill_PurchaseBillId",
                        column: x => x.PurchaseBillId,
                        principalTable: "Purchase_Bill",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Location_Details_Location_Code",
                table: "Location_Details",
                column: "Location_Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchase_Bill_Item_PurchaseBillId",
                table: "Purchase_Bill_Item",
                column: "PurchaseBillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Location_Details");

            migrationBuilder.DropTable(
                name: "Purchase_Bill_Item");

            migrationBuilder.DropTable(
                name: "Purchase_Bill");
        }
    }
}
