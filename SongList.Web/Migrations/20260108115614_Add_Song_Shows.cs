using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SongList.Web.Migrations
{
    /// <inheritdoc />
    public partial class Add_Song_Shows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SongShowId",
                table: "SlideHistory",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SongShows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HolyricsSongId = table.Column<int>(type: "integer", nullable: false),
                    ShowedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    HiddenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongShows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SongShows_HolyricsSongs_HolyricsSongId",
                        column: x => x.HolyricsSongId,
                        principalTable: "HolyricsSongs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SlideHistory_HiddenAt",
                table: "SlideHistory",
                column: "HiddenAt");

            migrationBuilder.CreateIndex(
                name: "IX_SlideHistory_SongShowId",
                table: "SlideHistory",
                column: "SongShowId");

            migrationBuilder.CreateIndex(
                name: "IX_SongShows_HolyricsSongId",
                table: "SongShows",
                column: "HolyricsSongId");

            migrationBuilder.AddForeignKey(
                name: "FK_SlideHistory_SongShows_SongShowId",
                table: "SlideHistory",
                column: "SongShowId",
                principalTable: "SongShows",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SlideHistory_SongShows_SongShowId",
                table: "SlideHistory");

            migrationBuilder.DropTable(
                name: "SongShows");

            migrationBuilder.DropIndex(
                name: "IX_SlideHistory_HiddenAt",
                table: "SlideHistory");

            migrationBuilder.DropIndex(
                name: "IX_SlideHistory_SongShowId",
                table: "SlideHistory");

            migrationBuilder.DropColumn(
                name: "SongShowId",
                table: "SlideHistory");
        }
    }
}
