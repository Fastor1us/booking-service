using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_bookings_status_valid",
                table: "bookings");

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "bookings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    login = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    password_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.CheckConstraint("ck_login", "LENGTH(login) >= 5");
                    table.CheckConstraint("ck_users_role_valid", "role IN ('User', 'Admin')");
                });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_user_id",
                table: "bookings",
                column: "user_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_bookings_status_valid",
                table: "bookings",
                sql: "status IN ('Pending', 'Confirmed', 'Rejected', 'Cancelled')");

            migrationBuilder.CreateIndex(
                name: "ix_users_login",
                table: "users",
                column: "login",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_users_bookings",
                table: "bookings",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_users_bookings",
                table: "bookings");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropIndex(
                name: "IX_bookings_user_id",
                table: "bookings");

            migrationBuilder.DropCheckConstraint(
                name: "ck_bookings_status_valid",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "bookings");

            migrationBuilder.AddCheckConstraint(
                name: "ck_bookings_status_valid",
                table: "bookings",
                sql: "status IN ('Pending', 'Confirmed', 'Rejected')");
        }
    }
}
