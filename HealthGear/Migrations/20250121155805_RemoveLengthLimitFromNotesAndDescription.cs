using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthGear.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLengthLimitFromNotesAndDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MaintenanceType",
                table: "Maintenance",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaintenanceType",
                table: "Maintenance");
        }
    }
}
