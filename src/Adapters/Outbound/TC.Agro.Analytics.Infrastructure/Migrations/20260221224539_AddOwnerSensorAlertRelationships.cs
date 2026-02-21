using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TC.Agro.Analytics.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerSensorAlertRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_alerts_plot_id",
                schema: "public",
                table: "alerts");

            migrationBuilder.DropIndex(
                name: "ix_alerts_plot_id_status",
                schema: "public",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "plot_id",
                schema: "public",
                table: "alerts");

            migrationBuilder.CreateTable(
                name: "owner_snapshots",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_owner_snapshots", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sensor_snapshots",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    property_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    plot_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    property_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sensor_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "fk_sensor_snapshots_owner_snapshots_owner_id",
                        column: x => x.owner_id,
                        principalSchema: "public",
                        principalTable: "owner_snapshots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_owner_snapshots_email",
                schema: "public",
                table: "owner_snapshots",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sensor_snapshots_owner_id",
                schema: "public",
                table: "sensor_snapshots",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_sensor_snapshots_owner_id_is_active",
                schema: "public",
                table: "sensor_snapshots",
                columns: new[] { "owner_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_sensor_snapshots_plot_id",
                schema: "public",
                table: "sensor_snapshots",
                column: "plot_id");

            migrationBuilder.CreateIndex(
                name: "ix_sensor_snapshots_plot_id_is_active",
                schema: "public",
                table: "sensor_snapshots",
                columns: new[] { "plot_id", "is_active" });

            migrationBuilder.AddForeignKey(
                name: "fk_alerts_sensor_snapshots_sensor_id",
                schema: "public",
                table: "alerts",
                column: "sensor_id",
                principalSchema: "public",
                principalTable: "sensor_snapshots",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_alerts_sensor_snapshots_sensor_id",
                schema: "public",
                table: "alerts");

            migrationBuilder.DropTable(
                name: "sensor_snapshots",
                schema: "public");

            migrationBuilder.DropTable(
                name: "owner_snapshots",
                schema: "public");

            migrationBuilder.AddColumn<Guid>(
                name: "plot_id",
                schema: "public",
                table: "alerts",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.CreateIndex(
                name: "ix_alerts_plot_id",
                schema: "public",
                table: "alerts",
                column: "plot_id");

            migrationBuilder.CreateIndex(
                name: "ix_alerts_plot_id_status",
                schema: "public",
                table: "alerts",
                columns: new[] { "plot_id", "status" });
        }
    }
}
