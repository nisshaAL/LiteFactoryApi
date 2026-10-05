using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiteFactoryApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                migrationBuilder.AddColumn<string>(
                    name: "Role",
                    table: "Users",
                    type: "character varying(20)",
                    maxLength: 20,
                    nullable: false,
                    defaultValue: "USER");
            }
            else
            {
                migrationBuilder.AddColumn<string>(
                    name: "Role",
                    table: "Users",
                    type: "TEXT",
                    maxLength: 20,
                    nullable: false,
                    defaultValue: "USER");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");
        }
    }
}
