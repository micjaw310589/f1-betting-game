using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace F1BettingApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverChampionshipTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DriverChampionships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DriverId = table.Column<int>(type: "integer", nullable: false),
                    Season = table.Column<int>(type: "integer", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverChampionships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverChampionships_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DriverChampionshipRaces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DriverChampionshipId = table.Column<int>(type: "integer", nullable: false),
                    RaceId = table.Column<int>(type: "integer", nullable: false),
                    PointsEarned = table.Column<int>(type: "integer", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverChampionshipRaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverChampionshipRaces_DriverChampionships_DriverChampions~",
                        column: x => x.DriverChampionshipId,
                        principalTable: "DriverChampionships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DriverChampionshipRaces_Races_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Races",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DriverChampionshipRaces_DriverChampionshipId",
                table: "DriverChampionshipRaces",
                column: "DriverChampionshipId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverChampionshipRaces_RaceId",
                table: "DriverChampionshipRaces",
                column: "RaceId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverChampionships_DriverId_Season",
                table: "DriverChampionships",
                columns: new[] { "DriverId", "Season" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverChampionshipRaces");

            migrationBuilder.DropTable(
                name: "DriverChampionships");
        }
    }
}
