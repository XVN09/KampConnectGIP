using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KampConnect.Identity.Data.Context.Migrations
{
    /// <inheritdoc />
    public partial class InitialIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "claims");

            migrationBuilder.EnsureSchema(
                name: "users");

            migrationBuilder.EnsureSchema(
                name: "oidc");

            migrationBuilder.EnsureSchema(
                name: "roles");

            migrationBuilder.EnsureSchema(
                name: "platform_security");

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FriendlyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Xml = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sec_claim",
                schema: "claims",
                columns: table => new
                {
                    claim_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    claim_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    claim_description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    creation_timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modification_timestamp = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sec_claim", x => x.claim_id);
                });

            migrationBuilder.CreateTable(
                name: "sec_oidc_application",
                schema: "oidc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ClientSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ConsentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayNames = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JsonWebKeySet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Permissions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostLogoutRedirectUris = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RedirectUris = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Requirements = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Settings = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sec_oidc_application", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sec_oidc_scope",
                schema: "oidc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConcurrencyToken = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Descriptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayNames = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Resources = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sec_oidc_scope", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sec_role",
                schema: "roles",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    role_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    role_description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_platform_role = table.Column<bool>(type: "bit", nullable: false),
                    creation_timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modification_timestamp = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sec_role", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "sec_tenant_identity_provider",
                schema: "platform_security",
                columns: table => new
                {
                    tenant_identity_provider_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tenant_id = table.Column<int>(type: "int", nullable: false),
                    issuer = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    provider_client_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    email_domain = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    creation_timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modification_timestamp = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sec_tenant_identity_provider", x => x.tenant_identity_provider_id);
                });

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

            migrationBuilder.CreateTable(
                name: "sec_user",
                schema: "users",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    last_login = table.Column<DateTime>(type: "datetime2", nullable: true),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    email_normalized = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    email_verified_timestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    phone_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    failed_login_count = table.Column<int>(type: "int", nullable: false),
                    locked_until = table.Column<DateTime>(type: "datetime2", nullable: true),
                    creation_timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modification_timestamp = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sec_user", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "sec_oidc_authorization",
                schema: "oidc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Scopes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sec_oidc_authorization", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sec_oidc_authorization_sec_oidc_application_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "oidc",
                        principalTable: "sec_oidc_application",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "sec_external_identity",
                schema: "users",
                columns: table => new
                {
                    external_identity_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    issuer = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    subject = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    provider_label = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    tenant_identity_provider_id = table.Column<int>(type: "int", nullable: true),
                    email_at_link = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    last_login = table.Column<DateTime>(type: "datetime2", nullable: true),
                    creation_timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modification_timestamp = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sec_external_identity", x => x.external_identity_id);
                    table.ForeignKey(
                        name: "FK_sec_external_identity_sec_tenant_identity_provider_tenant_identity_provider_id",
                        column: x => x.tenant_identity_provider_id,
                        principalSchema: "platform_security",
                        principalTable: "sec_tenant_identity_provider",
                        principalColumn: "tenant_identity_provider_id");
                    table.ForeignKey(
                        name: "FK_sec_external_identity_sec_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "users",
                        principalTable: "sec_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sec_login_attempt",
                schema: "users",
                columns: table => new
                {
                    login_attempt_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    attempted_username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    is_successful = table.Column<bool>(type: "bit", nullable: false),
                    failure_reason = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    creation_timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sec_login_attempt", x => x.login_attempt_id);
                    table.ForeignKey(
                        name: "FK_sec_login_attempt_sec_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "users",
                        principalTable: "sec_user",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "sec_user_claim",
                schema: "claims",
                columns: table => new
                {
                    user_claim_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    claim_id = table.Column<int>(type: "int", nullable: false),
                    claim_value = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    creation_timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modification_timestamp = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sec_user_claim", x => x.user_claim_id);
                    table.ForeignKey(
                        name: "FK_sec_user_claim_sec_claim_claim_id",
                        column: x => x.claim_id,
                        principalSchema: "claims",
                        principalTable: "sec_claim",
                        principalColumn: "claim_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sec_user_claim_sec_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "users",
                        principalTable: "sec_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sec_user_mfa",
                schema: "users",
                columns: table => new
                {
                    user_mfa_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    mfa_type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    secret_encrypted = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    is_confirmed = table.Column<bool>(type: "bit", nullable: false),
                    last_used = table.Column<DateTime>(type: "datetime2", nullable: true),
                    creation_timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modification_timestamp = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sec_user_mfa", x => x.user_mfa_id);
                    table.ForeignKey(
                        name: "FK_sec_user_mfa_sec_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "users",
                        principalTable: "sec_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sec_user_role",
                schema: "roles",
                columns: table => new
                {
                    user_role_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tenant_id = table.Column<int>(type: "int", nullable: true),
                    creation_timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modification_timestamp = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sec_user_role", x => x.user_role_id);
                    table.ForeignKey(
                        name: "FK_sec_user_role_sec_role_role_id",
                        column: x => x.role_id,
                        principalSchema: "roles",
                        principalTable: "sec_role",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sec_user_role_sec_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "users",
                        principalTable: "sec_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sec_user_token",
                schema: "users",
                columns: table => new
                {
                    user_token_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    token_hash = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    token_purpose = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    expiration_timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    consumed_timestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    creation_timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sec_user_token", x => x.user_token_id);
                    table.ForeignKey(
                        name: "FK_sec_user_token_sec_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "users",
                        principalTable: "sec_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_invitation",
                schema: "roles",
                columns: table => new
                {
                    tenant_invitation_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tenant_id = table.Column<int>(type: "int", nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    sent_by_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    accepted_by_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    cancelled_by_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    token_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    creation_timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modification_timestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    accepted_timestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    cancelled_timestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    expiry_timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_invitation", x => x.tenant_invitation_id);
                    table.ForeignKey(
                        name: "FK_tenant_invitation_sec_role_role_id",
                        column: x => x.role_id,
                        principalSchema: "roles",
                        principalTable: "sec_role",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_invitation_sec_tenant_snapshot_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "roles",
                        principalTable: "sec_tenant_snapshot",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_invitation_sec_user_accepted_by_user_id",
                        column: x => x.accepted_by_user_id,
                        principalSchema: "users",
                        principalTable: "sec_user",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_tenant_invitation_sec_user_cancelled_by_user_id",
                        column: x => x.cancelled_by_user_id,
                        principalSchema: "users",
                        principalTable: "sec_user",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_tenant_invitation_sec_user_sent_by_user_id",
                        column: x => x.sent_by_user_id,
                        principalSchema: "users",
                        principalTable: "sec_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sec_oidc_token",
                schema: "oidc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: true),
                    AuthorizationId = table.Column<int>(type: "int", nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RedemptionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReferenceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sec_oidc_token", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sec_oidc_token_sec_oidc_application_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "oidc",
                        principalTable: "sec_oidc_application",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_sec_oidc_token_sec_oidc_authorization_AuthorizationId",
                        column: x => x.AuthorizationId,
                        principalSchema: "oidc",
                        principalTable: "sec_oidc_authorization",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_sec_external_identity_issuer_subject",
                schema: "users",
                table: "sec_external_identity",
                columns: new[] { "issuer", "subject" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sec_external_identity_tenant_identity_provider_id",
                schema: "users",
                table: "sec_external_identity",
                column: "tenant_identity_provider_id");

            migrationBuilder.CreateIndex(
                name: "IX_sec_external_identity_user_id",
                schema: "users",
                table: "sec_external_identity",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_sec_login_attempt_user_id",
                schema: "users",
                table: "sec_login_attempt",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_sec_oidc_application_ClientId",
                schema: "oidc",
                table: "sec_oidc_application",
                column: "ClientId",
                unique: true,
                filter: "[ClientId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_sec_oidc_authorization_ApplicationId_Status_Subject_Type",
                schema: "oidc",
                table: "sec_oidc_authorization",
                columns: new[] { "ApplicationId", "Status", "Subject", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_sec_oidc_scope_Name",
                schema: "oidc",
                table: "sec_oidc_scope",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_sec_oidc_token_ApplicationId_Status_Subject_Type",
                schema: "oidc",
                table: "sec_oidc_token",
                columns: new[] { "ApplicationId", "Status", "Subject", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_sec_oidc_token_AuthorizationId",
                schema: "oidc",
                table: "sec_oidc_token",
                column: "AuthorizationId");

            migrationBuilder.CreateIndex(
                name: "IX_sec_oidc_token_ReferenceId",
                schema: "oidc",
                table: "sec_oidc_token",
                column: "ReferenceId",
                unique: true,
                filter: "[ReferenceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_sec_role_role_code",
                schema: "roles",
                table: "sec_role",
                column: "role_code",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_sec_user_email_normalized",
                schema: "users",
                table: "sec_user",
                column: "email_normalized",
                unique: true,
                filter: "[email_normalized] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_sec_user_claim_claim_id",
                schema: "claims",
                table: "sec_user_claim",
                column: "claim_id");

            migrationBuilder.CreateIndex(
                name: "IX_sec_user_claim_user_id",
                schema: "claims",
                table: "sec_user_claim",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_sec_user_mfa_user_id",
                schema: "users",
                table: "sec_user_mfa",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_sec_user_role_role_id",
                schema: "roles",
                table: "sec_user_role",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_sec_user_role_user_id_role_id_tenant_id",
                schema: "roles",
                table: "sec_user_role",
                columns: new[] { "user_id", "role_id", "tenant_id" },
                unique: true,
                filter: "[tenant_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_sec_user_token_token_hash",
                schema: "users",
                table: "sec_user_token",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sec_user_token_user_id",
                schema: "users",
                table: "sec_user_token",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invitation_accepted_by_user_id",
                schema: "roles",
                table: "tenant_invitation",
                column: "accepted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invitation_cancelled_by_user_id",
                schema: "roles",
                table: "tenant_invitation",
                column: "cancelled_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invitation_role_id",
                schema: "roles",
                table: "tenant_invitation",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invitation_sent_by_user_id",
                schema: "roles",
                table: "tenant_invitation",
                column: "sent_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invitation_tenant_id_email",
                schema: "roles",
                table: "tenant_invitation",
                columns: new[] { "tenant_id", "email" },
                unique: true,
                filter: "[status] = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invitation_token_hash",
                schema: "roles",
                table: "tenant_invitation",
                column: "token_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropTable(
                name: "sec_external_identity",
                schema: "users");

            migrationBuilder.DropTable(
                name: "sec_login_attempt",
                schema: "users");

            migrationBuilder.DropTable(
                name: "sec_oidc_scope",
                schema: "oidc");

            migrationBuilder.DropTable(
                name: "sec_oidc_token",
                schema: "oidc");

            migrationBuilder.DropTable(
                name: "sec_user_claim",
                schema: "claims");

            migrationBuilder.DropTable(
                name: "sec_user_mfa",
                schema: "users");

            migrationBuilder.DropTable(
                name: "sec_user_role",
                schema: "roles");

            migrationBuilder.DropTable(
                name: "sec_user_token",
                schema: "users");

            migrationBuilder.DropTable(
                name: "tenant_invitation",
                schema: "roles");

            migrationBuilder.DropTable(
                name: "sec_tenant_identity_provider",
                schema: "platform_security");

            migrationBuilder.DropTable(
                name: "sec_oidc_authorization",
                schema: "oidc");

            migrationBuilder.DropTable(
                name: "sec_claim",
                schema: "claims");

            migrationBuilder.DropTable(
                name: "sec_role",
                schema: "roles");

            migrationBuilder.DropTable(
                name: "sec_tenant_snapshot",
                schema: "roles");

            migrationBuilder.DropTable(
                name: "sec_user",
                schema: "users");

            migrationBuilder.DropTable(
                name: "sec_oidc_application",
                schema: "oidc");
        }
    }
}
