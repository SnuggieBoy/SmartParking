using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Infrastructure.Migrations;

/// <summary>
/// Adds VN admin codes to ParkingLocations for exact filtering.
/// NOTE: This project did not previously track migrations, so we keep this migration minimal
/// (only adds columns + index) to be safe against existing databases.
/// </summary>
public partial class AddProvinceWardCodesToParkingLocations : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "ProvinceCode",
            table: "ParkingLocations",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "WardCode",
            table: "ParkingLocations",
            type: "int",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_ParkingLocations_ProvinceCode_WardCode",
            table: "ParkingLocations",
            columns: new[] { "ProvinceCode", "WardCode" },
            filter: "[IsDeleted] = 0");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_ParkingLocations_ProvinceCode_WardCode",
            table: "ParkingLocations");

        migrationBuilder.DropColumn(
            name: "ProvinceCode",
            table: "ParkingLocations");

        migrationBuilder.DropColumn(
            name: "WardCode",
            table: "ParkingLocations");
    }
}

