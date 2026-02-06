using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TC.Agro.Analytics.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertsReadModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "analytics");

            migrationBuilder.CreateTable(
                name: "alerts",
                schema: "analytics",
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    acknowledged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    acknowledged_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolved_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    resolution_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_alerts", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_alerts_created_at",
                schema: "analytics",
                table: "alerts",
                column: "created_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "ix_alerts_plot_id",
                schema: "analytics",
                table: "alerts",
                column: "plot_id");

            migrationBuilder.CreateIndex(
                name: "ix_alerts_plot_status_created",
                schema: "analytics",
                table: "alerts",
                columns: new[] { "plot_id", "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_alerts_sensor_id",
                schema: "analytics",
                table: "alerts",
                column: "sensor_id");

            migrationBuilder.CreateIndex(
                name: "ix_alerts_sensor_reading_id",
                schema: "analytics",
                table: "alerts",
                column: "sensor_reading_id");

            migrationBuilder.CreateIndex(
                name: "ix_alerts_status",
                schema: "analytics",
                table: "alerts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_alerts_type",
                schema: "analytics",
                table: "alerts",
                column: "alert_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alerts",
                schema: "analytics");
        }
    }
}
