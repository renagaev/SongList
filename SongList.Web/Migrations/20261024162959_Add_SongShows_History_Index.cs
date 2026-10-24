using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SongList.Web.Migrations
{
    /// <inheritdoc />
    public partial class Add_SongShows_History_Index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SongShows_HolyricsSongId_ShowedAt",
                table: "SongShows",
                columns: new[] { "HolyricsSongId", "ShowedAt" },
                filter: "\"IsServiceModel\" IS TRUE OR \"IsServiceFix\" IS TRUE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SongShows_HolyricsSongId_ShowedAt",
                table: "SongShows");
        }
    }
}
