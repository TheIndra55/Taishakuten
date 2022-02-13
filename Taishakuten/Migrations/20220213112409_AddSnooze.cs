using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taishakuten.Migrations
{
    public partial class AddSnooze : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Snoozes",
                table: "Reminders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Snoozes",
                table: "Reminders");
        }
    }
}
