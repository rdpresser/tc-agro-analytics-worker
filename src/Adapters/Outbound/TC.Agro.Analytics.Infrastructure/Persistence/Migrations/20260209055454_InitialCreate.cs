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

            migrationBuilder.CreateTable(
                name: "alerts",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sensor_reading_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sensor_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    plot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    alert_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Medium"),
                    value = table.Column<double>(type: "double precision", nullable: true),
                    threshold = table.Column<double>(type: "double precision", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    acknowledged_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    acknowledged_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    resolved_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    resolution_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_alerts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sensor_readings",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sensor_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    plot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    time = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    temperature = table.Column<double>(type: "double precision", nullable: true),
                    humidity = table.Column<double>(type: "double precision", nullable: true),
                    soil_moisture = table.Column<double>(type: "double precision", nullable: true),
                    rainfall = table.Column<double>(type: "double precision", nullable: true),
                    battery_level = table.Column<double>(type: "double precision", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sensor_readings", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_alerts_created_at",
                schema: "public",
                table: "alerts",
                column: "created_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "ix_alerts_plot_id",
                schema: "public",
                table: "alerts",
                column: "plot_id");

            migrationBuilder.CreateIndex(
                name: "ix_alerts_plot_status_created",
                schema: "public",
                table: "alerts",
                columns: new[] { "plot_id", "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_alerts_sensor_id",
                schema: "public",
                table: "alerts",
                column: "sensor_id");

            migrationBuilder.CreateIndex(
                name: "ix_alerts_sensor_reading_id",
                schema: "public",
                table: "alerts",
                column: "sensor_reading_id");

            migrationBuilder.CreateIndex(
                name: "ix_alerts_status",
                schema: "public",
                table: "alerts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_alerts_type",
                schema: "public",
                table: "alerts",
                column: "alert_type");

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
                name: "alerts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sensor_readings",
                schema: "public");
        }
    }
}
