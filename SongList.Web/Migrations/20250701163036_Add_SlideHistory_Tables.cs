using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SongList.Web.Migrations
{
    /// <inheritdoc />
    public partial class Add_SlideHistory_Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SlideHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HolyricsId = table.Column<string>(type: "text", nullable: false),
                    SlideNumber = table.Column<int>(type: "integer", nullable: false),
                    TotalSlides = table.Column<int>(type: "integer", nullable: false),
                    SongName = table.Column<string>(type: "text", nullable: false),
                    ShowedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    HiddenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlideHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VerseHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Book = table.Column<int>(type: "integer", nullable: false),
                    Chapter = table.Column<int>(type: "integer", nullable: false),
                    Verse = table.Column<int>(type: "integer", nullable: false),
                    ShowedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    HiddenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerseHistory", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SlideHistory_HolyricsId",
                table: "SlideHistory",
                column: "HolyricsId");

            migrationBuilder.CreateIndex(
                name: "IX_SlideHistory_ShowedAt",
                table: "SlideHistory",
                column: "ShowedAt");

            migrationBuilder.CreateIndex(
                name: "IX_VerseHistory_ShowedAt",
                table: "VerseHistory",
                column: "ShowedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SlideHistory");

            migrationBuilder.DropTable(
                name: "VerseHistory");
        }
    }
}
