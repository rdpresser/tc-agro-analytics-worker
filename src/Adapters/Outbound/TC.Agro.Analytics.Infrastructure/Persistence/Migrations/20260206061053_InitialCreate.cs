using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TC.Agro.Analytics.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "alerts",
                schema: "analytics",
                newName: "alerts",
                newSchema: "public");

            migrationBuilder.CreateTable(
                name: "sensor_readings",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sensor_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    plot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    temperature = table.Column<double>(type: "double precision", nullable: false),
                    humidity = table.Column<double>(type: "double precision", nullable: false),
                    soil_moisture = table.Column<double>(type: "double precision", nullable: false),
                    rainfall = table.Column<double>(type: "double precision", nullable: false),
                    battery_level = table.Column<double>(type: "double precision", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sensor_readings", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_sensor_readings_plot_id",
                schema: "public",
                table: "sensor_readings",
                column: "plot_id");

            migrationBuilder.CreateIndex(
                name: "ix_sensor_readings_sensor_id",
                schema: "public",
                table: "sensor_readings",
                column: "sensor_id");

            migrationBuilder.CreateIndex(
                name: "ix_sensor_readings_time",
                schema: "public",
                table: "sensor_readings",
                column: "time");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sensor_readings",
                schema: "public");

            migrationBuilder.EnsureSchema(
                name: "analytics");

            migrationBuilder.RenameTable(
                name: "alerts",
                schema: "public",
                newName: "alerts",
                newSchema: "analytics");
        }
    }
}
