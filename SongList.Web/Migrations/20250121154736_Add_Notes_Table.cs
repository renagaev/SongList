using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SongList.Web.Migrations
{
    /// <inheritdoc />
    public partial class Add_Notes_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NoteId",
                table: "Song",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SimpleName = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Song_NoteId",
                table: "Song",
                column: "NoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Song_Notes_NoteId",
                table: "Song",
                column: "NoteId",
                principalTable: "Notes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Song_Notes_NoteId",
                table: "Song");

            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Song_NoteId",
                table: "Song");

            migrationBuilder.DropColumn(
                name: "NoteId",
                table: "Song");
        }
    }
}
