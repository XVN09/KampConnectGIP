using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KampConnect.Identity.Data.Context.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantSnapshotTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sec_tenant_snapshot",
                schema: "roles",
                columns: table => new
                {
                    tenant_id = table.Column<int>(type: "int", nullable: false),
                    tenant_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    tenant_slug = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    last_synced_timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TenantSnapshottenant_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sec_tenant_snapshot", x => x.tenant_id);
                    table.ForeignKey(
                        name: "FK_sec_tenant_snapshot_sec_tenant_snapshot_TenantSnapshottenant_id",
                        column: x => x.TenantSnapshottenant_id,
                        principalSchema: "roles",
                        principalTable: "sec_tenant_snapshot",
                        principalColumn: "tenant_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_sec_tenant_snapshot_tenant_id",
                schema: "roles",
                table: "sec_tenant_snapshot",
                column: "tenant_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sec_tenant_snapshot_TenantSnapshottenant_id",
                schema: "roles",
                table: "sec_tenant_snapshot",
                column: "TenantSnapshottenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sec_tenant_snapshot",
                schema: "roles");
        }
    }
}
