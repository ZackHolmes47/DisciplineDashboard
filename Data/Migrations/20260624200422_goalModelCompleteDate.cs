using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DisciplineDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class goalModelCompleteDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Goals",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Goals");
        }
    }
}
