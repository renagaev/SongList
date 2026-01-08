using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SongList.Web.Migrations
{
    /// <inheritdoc />
    public partial class Rename_Mappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(name: "Mappings", newName: "HolyricsSongs");
            
            migrationBuilder.AddColumn<int>(
                name: "HolyricsSongId",
                table: "SlideHistory",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HolyricsSongId",
                table: "History",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
-- Добавление недостающих записей в HolyricsSongs из SlideHistory
INSERT INTO ""HolyricsSongs"" (""HolyricsId"", ""Title"")
SELECT DISTINCT sh.""HolyricsId"", sh.""SongName""
FROM ""SlideHistory"" sh
WHERE NOT EXISTS (
    SELECT 1
    FROM ""HolyricsSongs"" hs
    WHERE hs.""HolyricsId"" = sh.""HolyricsId""
);

-- Заполнение HolyricsSongId в SlideHistory
UPDATE ""SlideHistory"" sh
SET ""HolyricsSongId"" = hs.""Id""
FROM ""HolyricsSongs"" hs
WHERE sh.""HolyricsId"" = hs.""HolyricsId"";

-- Заполнение HolyricsSongId в History
UPDATE ""History"" h
SET ""HolyricsSongId"" = hs.""Id""
FROM ""HolyricsSongs"" hs
WHERE h.""HolyricsId"" = hs.""HolyricsId"";");
            
            
            migrationBuilder.DropIndex(
                name: "IX_SlideHistory_HolyricsId",
                table: "SlideHistory");

            migrationBuilder.DropColumn(
                name: "HolyricsId",
                table: "SlideHistory");
            
            migrationBuilder.DropColumn(
                name: "SongName",
                table: "SlideHistory");
            
            migrationBuilder.DropColumn(
                name: "HolyricsId",
                table: "History");
            

            migrationBuilder.CreateIndex(
                name: "IX_SlideHistory_HolyricsSongId",
                table: "SlideHistory",
                column: "HolyricsSongId");

            migrationBuilder.CreateIndex(
                name: "IX_History_HolyricsSongId",
                table: "History",
                column: "HolyricsSongId");

            migrationBuilder.CreateIndex(
                name: "IX_HolyricsSongs_HolyricsId",
                table: "HolyricsSongs",
                column: "HolyricsId");

            migrationBuilder.CreateIndex(
                name: "IX_HolyricsSongs_SongId_HolyricsId",
                table: "HolyricsSongs",
                columns: new[] { "SongId", "HolyricsId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_History_HolyricsSongs_HolyricsSongId",
                table: "History",
                column: "HolyricsSongId",
                principalTable: "HolyricsSongs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SlideHistory_HolyricsSongs_HolyricsSongId",
                table: "SlideHistory",
                column: "HolyricsSongId",
                principalTable: "HolyricsSongs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotImplementedException();
        }
    }
}
