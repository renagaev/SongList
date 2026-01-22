using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SongList.Web.Migrations
{
    /// <inheritdoc />
    public partial class Add_Holyrics_Sync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Formatting",
                table: "HolyricsSongs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Lyrics",
                table: "HolyricsSongs",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HolyricsToken",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccessToken = table.Column<string>(type: "text", nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: false),
                    ExpiresIn = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HolyricsToken", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HolyricsToken");

            migrationBuilder.DropColumn(
                name: "Formatting",
                table: "HolyricsSongs");

            migrationBuilder.DropColumn(
                name: "Lyrics",
                table: "HolyricsSongs");
        }
    }
}
