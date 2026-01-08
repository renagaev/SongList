using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SongList.Web.Migrations
{
    /// <inheritdoc />
    public partial class Add_Model_Fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsServiceFix",
                table: "SongShows",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsServiceModel",
                table: "SongShows",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsServiceTrain",
                table: "SongShows",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Score",
                table: "SongShows",
                type: "real",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsServiceFix",
                table: "SongShows");

            migrationBuilder.DropColumn(
                name: "IsServiceModel",
                table: "SongShows");

            migrationBuilder.DropColumn(
                name: "IsServiceTrain",
                table: "SongShows");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "SongShows");
        }
    }
}
