using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthGear.Migrations.SettingsDb
{
    /// <inheritdoc />
    public partial class InitialSettingsMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppConfig",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Smtp_Host = table.Column<string>(type: "TEXT", nullable: false),
                    Smtp_Port = table.Column<int>(type: "INTEGER", nullable: false),
                    Smtp_Username = table.Column<string>(type: "TEXT", nullable: false),
                    Smtp_Password = table.Column<string>(type: "TEXT", nullable: false),
                    Smtp_UseSsl = table.Column<bool>(type: "INTEGER", nullable: false),
                    Smtp_RequiresAuthentication = table.Column<bool>(type: "INTEGER", nullable: false),
                    Smtp_SenderName = table.Column<string>(type: "TEXT", nullable: false),
                    Smtp_SenderEmail = table.Column<string>(type: "TEXT", nullable: false),
                    Logging_LogLevel = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppConfig", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppConfig");
        }
    }
}
