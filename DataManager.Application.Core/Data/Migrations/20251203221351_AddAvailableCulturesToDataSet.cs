using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataManager.Application.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAvailableCulturesToDataSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvailableCultures",
                table: "TranslationSets",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableCultures",
                table: "TranslationSets");
        }
    }
}
