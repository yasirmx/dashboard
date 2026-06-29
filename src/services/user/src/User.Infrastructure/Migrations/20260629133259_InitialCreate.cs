using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace User.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AvatarUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.Role });
                });

            migrationBuilder.InsertData(
                table: "Profiles",
                columns: new[] { "UserId", "AvatarUrl", "CreatedUtc", "Department", "Email", "FirstName", "IsActive", "LastName" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "https://i.pravatar.cc/150?u=alice@dashboard.local", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Engineering", "alice@dashboard.local", "Alice", true, "Walker" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "https://i.pravatar.cc/150?u=bob@dashboard.local", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Engineering", "bob@dashboard.local", "Bob", true, "Stone" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "https://i.pravatar.cc/150?u=carol@dashboard.local", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "QA", "carol@dashboard.local", "Carol", true, "Nguyen" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "https://i.pravatar.cc/150?u=dave@dashboard.local", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "QA", "dave@dashboard.local", "Dave", true, "Patel" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "https://i.pravatar.cc/150?u=erin@dashboard.local", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Product", "erin@dashboard.local", "Erin", true, "Lopez" },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "https://i.pravatar.cc/150?u=frank@dashboard.local", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Product", "frank@dashboard.local", "Frank", true, "Murphy" },
                    { new Guid("77777777-7777-7777-7777-777777777777"), "https://i.pravatar.cc/150?u=grace@dashboard.local", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Engineering", "grace@dashboard.local", "Grace", true, "Kim" },
                    { new Guid("88888888-8888-8888-8888-888888888888"), "https://i.pravatar.cc/150?u=henry@dashboard.local", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Engineering", "henry@dashboard.local", "Henry", true, "Adams" },
                    { new Guid("99999999-9999-9999-9999-999999999999"), "https://i.pravatar.cc/150?u=irene@dashboard.local", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Engineering", "irene@dashboard.local", "Irene", true, "Costa" },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "https://i.pravatar.cc/150?u=jack@dashboard.local", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "QA", "jack@dashboard.local", "Jack", true, "Owens" }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Role", "UserId" },
                values: new object[,]
                {
                    { "Dev", new Guid("11111111-1111-1111-1111-111111111111") },
                    { "Dev", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "QA", new Guid("33333333-3333-3333-3333-333333333333") },
                    { "Dev", new Guid("44444444-4444-4444-4444-444444444444") },
                    { "QA", new Guid("44444444-4444-4444-4444-444444444444") },
                    { "PO", new Guid("55555555-5555-5555-5555-555555555555") },
                    { "PO", new Guid("66666666-6666-6666-6666-666666666666") },
                    { "Lead", new Guid("77777777-7777-7777-7777-777777777777") },
                    { "Dev", new Guid("88888888-8888-8888-8888-888888888888") },
                    { "Lead", new Guid("88888888-8888-8888-8888-888888888888") },
                    { "Dev", new Guid("99999999-9999-9999-9999-999999999999") },
                    { "QA", new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_Email",
                table: "Profiles",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropTable(
                name: "UserRoles");
        }
    }
}
