using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthGear.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenancesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Maintenance_Devices_DeviceId",
                table: "Maintenance");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceDocuments_Maintenance_MaintenanceId",
                table: "MaintenanceDocuments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Maintenance",
                table: "Maintenance");

            migrationBuilder.RenameTable(
                name: "Maintenance",
                newName: "Maintenances");

            migrationBuilder.RenameIndex(
                name: "IX_Maintenance_DeviceId",
                table: "Maintenances",
                newName: "IX_Maintenances_DeviceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Maintenances",
                table: "Maintenances",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceDocuments_Maintenances_MaintenanceId",
                table: "MaintenanceDocuments",
                column: "MaintenanceId",
                principalTable: "Maintenances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Maintenances_Devices_DeviceId",
                table: "Maintenances",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceDocuments_Maintenances_MaintenanceId",
                table: "MaintenanceDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_Maintenances_Devices_DeviceId",
                table: "Maintenances");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Maintenances",
                table: "Maintenances");

            migrationBuilder.RenameTable(
                name: "Maintenances",
                newName: "Maintenance");

            migrationBuilder.RenameIndex(
                name: "IX_Maintenances_DeviceId",
                table: "Maintenance",
                newName: "IX_Maintenance_DeviceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Maintenance",
                table: "Maintenance",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Maintenance_Devices_DeviceId",
                table: "Maintenance",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceDocuments_Maintenance_MaintenanceId",
                table: "MaintenanceDocuments",
                column: "MaintenanceId",
                principalTable: "Maintenance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
