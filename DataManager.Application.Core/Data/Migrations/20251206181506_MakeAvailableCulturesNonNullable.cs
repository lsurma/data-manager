using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataManager.Application.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeAvailableCulturesNonNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AvailableCultures",
                table: "TranslationsSets",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AvailableCultures",
                table: "TranslationsSets",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
    }
}
