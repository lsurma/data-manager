using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataManager.Application.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceDataSetToTranslation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceTranslationSetId",
                table: "Translations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SourceDataSetLastSyncedAt",
                table: "Translations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Translations_SourceTranslationSetId",
                table: "Translations",
                column: "SourceTranslationSetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Translations_TranslationSets_SourceTranslationSetId",
                table: "Translations",
                column: "SourceTranslationSetId",
                principalTable: "TranslationSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Translations_TranslationSets_SourceTranslationSetId",
                table: "Translations");

            migrationBuilder.DropIndex(
                name: "IX_Translations_SourceTranslationSetId",
                table: "Translations");

            migrationBuilder.DropColumn(
                name: "SourceTranslationSetId",
                table: "Translations");

            migrationBuilder.DropColumn(
                name: "SourceDataSetLastSyncedAt",
                table: "Translations");
        }
    }
}
