using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Chittak.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "phone_verifications",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    phone = table.Column<string>(type: "text", nullable: false),
                    code_hash = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    consumed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_phone_verifications", x => x.id);
                    table.CheckConstraint("ck_phone_verifications_attempts", "attempts <= 5");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    phone = table.Column<string>(type: "text", nullable: false),
                    phone_hash = table.Column<string>(type: "text", nullable: false),
                    discoverable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.CheckConstraint("ck_users_phone_e164", "phone ~ '^\\+[1-9][0-9]{6,14}$'");
                });

            migrationBuilder.CreateTable(
                name: "devices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    platform = table.Column<string>(type: "text", nullable: true),
                    push_token = table.Column<string>(type: "text", nullable: true),
                    last_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_devices", x => x.id);
                    table.CheckConstraint("ck_devices_platform", "platform IN ('android', 'ios', 'linux', 'windows', 'macos')");
                    table.ForeignKey(
                        name: "fk_devices_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "auth_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    refresh_token_hash = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auth_sessions", x => x.id);
                    table.ForeignKey(
                        name: "fk_auth_sessions_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "device_identity_keys",
                columns: table => new
                {
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    identity_key = table.Column<byte[]>(type: "bytea", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_device_identity_keys", x => x.device_id);
                    table.CheckConstraint("ck_device_identity_keys_length", "octet_length(identity_key) = 64");
                    table.ForeignKey(
                        name: "fk_device_identity_keys_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "message_queue",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    recipient_device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_device_id = table.Column<Guid>(type: "uuid", nullable: true),
                    client_message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    envelope = table.Column<byte[]>(type: "bytea", nullable: false),
                    envelope_type = table.Column<short>(type: "smallint", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_message_queue", x => x.id);
                    table.CheckConstraint("ck_message_queue_envelope_type", "envelope_type IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "fk_message_queue_devices_recipient_device_id",
                        column: x => x.recipient_device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "one_time_prekeys",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    key_id = table.Column<int>(type: "integer", nullable: false),
                    public_key = table.Column<byte[]>(type: "bytea", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_one_time_prekeys", x => x.id);
                    table.CheckConstraint("ck_one_time_prekeys_public_key_length", "octet_length(public_key) = 32");
                    table.ForeignKey(
                        name: "fk_one_time_prekeys_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "signed_prekeys",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    key_id = table.Column<int>(type: "integer", nullable: false),
                    public_key = table.Column<byte[]>(type: "bytea", nullable: false),
                    signature = table.Column<byte[]>(type: "bytea", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_signed_prekeys", x => x.id);
                    table.CheckConstraint("ck_signed_prekeys_public_key_length", "octet_length(public_key) = 32");
                    table.CheckConstraint("ck_signed_prekeys_signature_length", "octet_length(signature) = 64");
                    table.ForeignKey(
                        name: "fk_signed_prekeys_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_auth_device",
                table: "auth_sessions",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "idx_devices_user",
                table: "devices",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_devices_user_id_registration_id",
                table: "devices",
                columns: new[] { "user_id", "registration_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_queue_expiry",
                table: "message_queue",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "idx_queue_recipient",
                table: "message_queue",
                columns: new[] { "recipient_device_id", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_message_queue_recipient_device_id_client_message_id",
                table: "message_queue",
                columns: new[] { "recipient_device_id", "client_message_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_otk_device",
                table: "one_time_prekeys",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "ix_one_time_prekeys_device_id_key_id",
                table: "one_time_prekeys",
                columns: new[] { "device_id", "key_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_phone_verif",
                table: "phone_verifications",
                columns: new[] { "phone", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_signed_prekeys_device",
                table: "signed_prekeys",
                columns: new[] { "device_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_signed_prekeys_device_id_key_id",
                table: "signed_prekeys",
                columns: new[] { "device_id", "key_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_phone",
                table: "users",
                column: "phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_phone_hash",
                table: "users",
                column: "phone_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auth_sessions");

            migrationBuilder.DropTable(
                name: "device_identity_keys");

            migrationBuilder.DropTable(
                name: "message_queue");

            migrationBuilder.DropTable(
                name: "one_time_prekeys");

            migrationBuilder.DropTable(
                name: "phone_verifications");

            migrationBuilder.DropTable(
                name: "signed_prekeys");

            migrationBuilder.DropTable(
                name: "devices");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
