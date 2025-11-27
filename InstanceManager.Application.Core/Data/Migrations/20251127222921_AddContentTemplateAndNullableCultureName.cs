using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstanceManager.Application.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddContentTemplateAndNullableCultureName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CultureName",
                table: "Translations",
                type: "TEXT",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<string>(
                name: "ContentTemplate",
                table: "Translations",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentTemplate",
                table: "Translations");

            migrationBuilder.AlterColumn<string>(
                name: "CultureName",
                table: "Translations",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 10,
                oldNullable: true);
        }
    }
}
