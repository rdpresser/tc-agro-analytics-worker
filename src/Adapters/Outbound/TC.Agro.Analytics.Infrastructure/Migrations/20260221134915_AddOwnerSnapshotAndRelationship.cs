using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TC.Agro.Analytics.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerSnapshotAndRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "owner_id",
                schema: "public",
                table: "alerts",
                type: "uuid",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "ix_alerts_owner_id",
                schema: "public",
                table: "alerts",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_owner_snapshots_email",
                schema: "public",
                table: "owner_snapshots",
                column: "email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_alerts_owner_snapshots_owner_id",
                schema: "public",
                table: "alerts",
                column: "owner_id",
                principalSchema: "public",
                principalTable: "owner_snapshots",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_alerts_owner_snapshots_owner_id",
                schema: "public",
                table: "alerts");

            migrationBuilder.DropTable(
                name: "owner_snapshots",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "ix_alerts_owner_id",
                schema: "public",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "owner_id",
                schema: "public",
                table: "alerts");
        }
    }
}
