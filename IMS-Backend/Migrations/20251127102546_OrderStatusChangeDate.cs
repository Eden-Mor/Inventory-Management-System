using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS_Backend.Migrations
{
    /// <inheritdoc />
    public partial class OrderStatusChangeDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StatusChangeDate",
                table: "SupplierOrders",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusChangeDate",
                table: "SupplierOrders");
        }
    }
}
