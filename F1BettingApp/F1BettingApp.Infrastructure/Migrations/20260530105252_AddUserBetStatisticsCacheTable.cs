using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace F1BettingApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserBetStatisticsCacheTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserBetStatisticsCaches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    TotalBets = table.Column<int>(type: "integer", nullable: false),
                    WinningBets = table.Column<int>(type: "integer", nullable: false),
                    LosingBets = table.Column<int>(type: "integer", nullable: false),
                    PushBets = table.Column<int>(type: "integer", nullable: false),
                    TotalWinnings = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalAmountBet = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CurrentWinStreak = table.Column<int>(type: "integer", nullable: false),
                    CurrentLoseStreak = table.Column<int>(type: "integer", nullable: false),
                    LongestWinStreak = table.Column<int>(type: "integer", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FavoriteDriverId = table.Column<int>(type: "integer", nullable: false),
                    LargestWin = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    LargestLoss = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBetStatisticsCaches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBetStatisticsCaches_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserBetStatisticsCaches_UserId",
                table: "UserBetStatisticsCaches",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBetStatisticsCaches");
        }
    }
}
