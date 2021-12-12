using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebServerTest.Migrations
{
    public partial class MigrationIdentifier : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Identifier",
                table: "Users",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Identifier",
                table: "Users");
        }
    }
}
