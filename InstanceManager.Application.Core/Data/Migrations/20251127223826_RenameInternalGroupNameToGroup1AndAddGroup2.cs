using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstanceManager.Application.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameInternalGroupNameToGroup1AndAddGroup2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Translations_InternalGroupName_ResourceName_CultureName",
                table: "Translations");

            migrationBuilder.RenameColumn(
                name: "InternalGroupName",
                table: "Translations",
                newName: "InternalGroupName1");

            migrationBuilder.AlterColumn<string>(
                name: "InternalGroupName1",
                table: "Translations",
                type: "TEXT",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "InternalGroupName2",
                table: "Translations",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Translations_InternalGroupName1_InternalGroupName2_ResourceName_CultureName",
                table: "Translations",
                columns: new[] { "InternalGroupName1", "InternalGroupName2", "ResourceName", "CultureName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Translations_InternalGroupName1_InternalGroupName2_ResourceName_CultureName",
                table: "Translations");

            migrationBuilder.DropColumn(
                name: "InternalGroupName2",
                table: "Translations");

            migrationBuilder.RenameColumn(
                name: "InternalGroupName1",
                table: "Translations",
                newName: "InternalGroupName");

            migrationBuilder.AlterColumn<string>(
                name: "InternalGroupName",
                table: "Translations",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Translations_InternalGroupName_ResourceName_CultureName",
                table: "Translations",
                columns: new[] { "InternalGroupName", "ResourceName", "CultureName" });
        }
    }
}
