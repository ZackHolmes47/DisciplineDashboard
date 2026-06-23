using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DisciplineDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateJoournalEntryModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WhatToImporve",
                table: "JournalEntries");

            migrationBuilder.RenameColumn(
                name: "WhatWentWell",
                table: "JournalEntries",
                newName: "TomorrowMission");

            migrationBuilder.AddColumn<string>(
                name: "Mood",
                table: "JournalEntries",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reflection",
                table: "JournalEntries",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "JournalEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatToImprove",
                table: "JournalEntries",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mood",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "Reflection",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "WhatToImprove",
                table: "JournalEntries");

            migrationBuilder.RenameColumn(
                name: "TomorrowMission",
                table: "JournalEntries",
                newName: "WhatWentWell");

            migrationBuilder.AddColumn<string>(
                name: "WhatToImporve",
                table: "JournalEntries",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
