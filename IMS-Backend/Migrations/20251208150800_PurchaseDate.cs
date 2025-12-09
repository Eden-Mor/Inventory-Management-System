using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS_Backend.Migrations;

/// <inheritdoc />
public partial class PurchaseDate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "PurchaseDate",
            table: "Purchases",
            type: "timestamp with time zone",
            nullable: false,
            defaultValueSql: "NOW() AT TIME ZONE 'UTC'");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PurchaseDate",
            table: "Purchases");
    }
}
