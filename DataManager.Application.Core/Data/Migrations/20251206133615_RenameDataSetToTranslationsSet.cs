using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataManager.Application.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameDataSetToTranslationsSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename tables
            migrationBuilder.RenameTable(
                name: "DataSets",
                newName: "TranslationsSets");

            migrationBuilder.RenameTable(
                name: "DataSetInclude",
                newName: "TranslationsSetsIncludes");

            // Rename columns in TranslationsSetsIncludes table
            migrationBuilder.RenameColumn(
                name: "ParentDataSetId",
                table: "TranslationsSetsIncludes",
                newName: "ParentTranslationsSetId");

            migrationBuilder.RenameColumn(
                name: "IncludedDataSetId",
                table: "TranslationsSetsIncludes",
                newName: "IncludedTranslationsSetId");

            // Rename column in Translations table
            migrationBuilder.RenameColumn(
                name: "DataSetId",
                table: "Translations",
                newName: "TranslationsSetId");

            // Rename indexes
            migrationBuilder.RenameIndex(
                name: "IX_Translations_DataSetId",
                table: "Translations",
                newName: "IX_Translations_TranslationsSetId");

            migrationBuilder.RenameIndex(
                name: "IX_DataSetInclude_IncludedDataSetId",
                table: "TranslationsSetsIncludes",
                newName: "IX_TranslationsSetsIncludes_IncludedTranslationsSetId");

            migrationBuilder.RenameIndex(
                name: "IX_DataSetInclude_ParentDataSetId_IncludedDataSetId",
                table: "TranslationsSetsIncludes",
                newName: "IX_TranslationsSetsIncludes_ParentTranslationsSetId_IncludedTranslationsSetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rename indexes back
            migrationBuilder.RenameIndex(
                name: "IX_TranslationsSetsIncludes_ParentTranslationsSetId_IncludedTranslationsSetId",
                table: "TranslationsSetsIncludes",
                newName: "IX_DataSetInclude_ParentDataSetId_IncludedDataSetId");

            migrationBuilder.RenameIndex(
                name: "IX_TranslationsSetsIncludes_IncludedTranslationsSetId",
                table: "TranslationsSetsIncludes",
                newName: "IX_DataSetInclude_IncludedDataSetId");

            migrationBuilder.RenameIndex(
                name: "IX_Translations_TranslationsSetId",
                table: "Translations",
                newName: "IX_Translations_DataSetId");

            // Rename column in Translations table back
            migrationBuilder.RenameColumn(
                name: "TranslationsSetId",
                table: "Translations",
                newName: "DataSetId");

            // Rename columns in TranslationsSetsIncludes table back
            migrationBuilder.RenameColumn(
                name: "IncludedTranslationsSetId",
                table: "TranslationsSetsIncludes",
                newName: "IncludedDataSetId");

            migrationBuilder.RenameColumn(
                name: "ParentTranslationsSetId",
                table: "TranslationsSetsIncludes",
                newName: "ParentDataSetId");

            // Rename tables back
            migrationBuilder.RenameTable(
                name: "TranslationsSetsIncludes",
                newName: "DataSetInclude");

            migrationBuilder.RenameTable(
                name: "TranslationsSets",
                newName: "DataSets");
        }
    }
}
