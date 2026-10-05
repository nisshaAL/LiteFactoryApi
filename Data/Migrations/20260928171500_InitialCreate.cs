using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiteFactoryApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                migrationBuilder.CreateTable(
                    name: "Users",
                    columns: table => new
                    {
                        Id = table.Column<Guid>(type: "uuid", nullable: false),
                        Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                        NormalizedEmail = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                        Nickname = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                        NormalizedNickname = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                        PasswordHash = table.Column<string>(type: "text", nullable: false),
                        CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                        IsActive = table.Column<bool>(type: "boolean", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Users", x => x.Id);
                    });
            }
            else
            {
                migrationBuilder.CreateTable(
                    name: "Users",
                    columns: table => new
                    {
                        Id = table.Column<Guid>(type: "TEXT", nullable: false),
                        Email = table.Column<string>(type: "TEXT", maxLength: 254, nullable: false),
                        NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 254, nullable: false),
                        Nickname = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                        NormalizedNickname = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                        PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                        CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                        IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Users", x => x.Id);
                    });
            }

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedNickname",
                table: "Users",
                column: "NormalizedNickname",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
