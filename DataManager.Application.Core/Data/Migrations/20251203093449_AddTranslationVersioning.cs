using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataManager.Application.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTranslationVersioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCurrentVersion",
                table: "Translations",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraftVersion",
                table: "Translations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOldVersion",
                table: "Translations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalTranslationId",
                table: "Translations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Translations_IsCurrentVersion_IsDraftVersion_IsOldVersion",
                table: "Translations",
                columns: new[] { "IsCurrentVersion", "IsDraftVersion", "IsOldVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_Translations_OriginalTranslationId",
                table: "Translations",
                column: "OriginalTranslationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Translations_Translations_OriginalTranslationId",
                table: "Translations",
                column: "OriginalTranslationId",
                principalTable: "Translations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Translations_Translations_OriginalTranslationId",
                table: "Translations");

            migrationBuilder.DropIndex(
                name: "IX_Translations_IsCurrentVersion_IsDraftVersion_IsOldVersion",
                table: "Translations");

            migrationBuilder.DropIndex(
                name: "IX_Translations_OriginalTranslationId",
                table: "Translations");

            migrationBuilder.DropColumn(
                name: "IsCurrentVersion",
                table: "Translations");

            migrationBuilder.DropColumn(
                name: "IsDraftVersion",
                table: "Translations");

            migrationBuilder.DropColumn(
                name: "IsOldVersion",
                table: "Translations");

            migrationBuilder.DropColumn(
                name: "OriginalTranslationId",
                table: "Translations");
        }
    }
}
