using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryIdToStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventory_Stocks_StockId",
                table: "Inventory");

            migrationBuilder.DropIndex(
                name: "IX_Inventory_StockId",
                table: "Inventory");

            migrationBuilder.AddColumn<int>(
                name: "InventoryId",
                table: "Stocks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_InventoryId",
                table: "Stocks",
                column: "InventoryId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Inventory_InventoryId",
                table: "Stocks",
                column: "InventoryId",
                principalTable: "Inventory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Inventory_InventoryId",
                table: "Stocks");

            migrationBuilder.DropIndex(
                name: "IX_Stocks_InventoryId",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "InventoryId",
                table: "Stocks");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_StockId",
                table: "Inventory",
                column: "StockId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventory_Stocks_StockId",
                table: "Inventory",
                column: "StockId",
                principalTable: "Stocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
