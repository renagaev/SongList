using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SongList.Web.Migrations
{
    /// <inheritdoc />
    public partial class Add_Holyrics_Partition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HolyricsPartitions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Md5 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HolyricsPartitions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HolyricsPartitions");
        }
    }
}
