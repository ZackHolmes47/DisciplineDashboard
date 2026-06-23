using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DisciplineDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class addHabitTargetFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "TargetValue",
                table: "Habits",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "Habits",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetValue",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Habits");
        }
    }
}
