using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DisciplineDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class addHabitLogProgressField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ActualValue",
                table: "HabitLogs",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "HabitLogs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualValue",
                table: "HabitLogs");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "HabitLogs");
        }
    }
}
