using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataManager.Application.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceSourceDataSetWithSourceTranslation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Translations_TranslationSets_SourceTranslationSetId",
                table: "Translations");

            migrationBuilder.RenameColumn(
                name: "SourceDataSetLastSyncedAt",
                table: "Translations",
                newName: "SourceTranslationLastSyncedAt");

            migrationBuilder.RenameColumn(
                name: "SourceTranslationSetId",
                table: "Translations",
                newName: "SourceTranslationId");

            migrationBuilder.RenameIndex(
                name: "IX_Translations_SourceTranslationSetId",
                table: "Translations",
                newName: "IX_Translations_SourceTranslationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Translations_Translations_SourceTranslationId",
                table: "Translations",
                column: "SourceTranslationId",
                principalTable: "Translations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Translations_Translations_SourceTranslationId",
                table: "Translations");

            migrationBuilder.RenameColumn(
                name: "SourceTranslationLastSyncedAt",
                table: "Translations",
                newName: "SourceDataSetLastSyncedAt");

            migrationBuilder.RenameColumn(
                name: "SourceTranslationId",
                table: "Translations",
                newName: "SourceTranslationSetId");

            migrationBuilder.RenameIndex(
                name: "IX_Translations_SourceTranslationId",
                table: "Translations",
                newName: "IX_Translations_SourceTranslationSetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Translations_TranslationSets_SourceTranslationSetId",
                table: "Translations",
                column: "SourceTranslationSetId",
                principalTable: "TranslationSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
