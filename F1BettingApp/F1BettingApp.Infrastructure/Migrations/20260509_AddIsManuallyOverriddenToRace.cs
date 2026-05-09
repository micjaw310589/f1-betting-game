using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1BettingApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsManuallyOverriddenToRace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsManuallyOverridden",
                table: "Races",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsManuallyOverridden",
                table: "Races");
        }
    }
}
