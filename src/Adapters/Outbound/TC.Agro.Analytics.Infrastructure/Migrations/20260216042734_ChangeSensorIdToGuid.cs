using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TC.Agro.Analytics.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeSensorIdToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add new temporary column
            migrationBuilder.AddColumn<Guid>(
                name: "sensor_id_new",
                schema: "public",
                table: "alerts",
                type: "uuid",
                nullable: true);

            // Step 2: Migrate data - convert string SensorId to UUID if it's a valid GUID
            // If you have existing data, you may need custom logic here
            migrationBuilder.Sql(@"
                UPDATE public.alerts 
                SET sensor_id_new = sensor_id::uuid 
                WHERE sensor_id ~ '^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$';
            ");

            // Step 3: Drop old column and index
            migrationBuilder.DropIndex(
                name: "ix_alerts_sensor_id",
                schema: "public",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "sensor_id",
                schema: "public",
                table: "alerts");

            // Step 4: Rename new column to sensor_id
            migrationBuilder.RenameColumn(
                name: "sensor_id_new",
                schema: "public",
                table: "alerts",
                newName: "sensor_id");

            // Step 5: Make column not nullable
            migrationBuilder.AlterColumn<Guid>(
                name: "sensor_id",
                schema: "public",
                table: "alerts",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            // Step 6: Recreate index
            migrationBuilder.CreateIndex(
                name: "ix_alerts_sensor_id",
                schema: "public",
                table: "alerts",
                column: "sensor_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Step 1: Drop index
            migrationBuilder.DropIndex(
                name: "ix_alerts_sensor_id",
                schema: "public",
                table: "alerts");

            // Step 2: Add temporary string column
            migrationBuilder.AddColumn<string>(
                name: "sensor_id_old",
                schema: "public",
                table: "alerts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            // Step 3: Migrate data back - convert UUID to string
            migrationBuilder.Sql(@"
                UPDATE public.alerts 
                SET sensor_id_old = sensor_id::text;
            ");

            // Step 4: Drop UUID column
            migrationBuilder.DropColumn(
                name: "sensor_id",
                schema: "public",
                table: "alerts");

            // Step 5: Rename old column back
            migrationBuilder.RenameColumn(
                name: "sensor_id_old",
                schema: "public",
                table: "alerts",
                newName: "sensor_id");

            // Step 6: Make column not nullable
            migrationBuilder.AlterColumn<string>(
                name: "sensor_id",
                schema: "public",
                table: "alerts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldNullable: true);

            // Step 7: Recreate index
            migrationBuilder.CreateIndex(
                name: "ix_alerts_sensor_id",
                schema: "public",
                table: "alerts",
                column: "sensor_id");
        }
    }
}
