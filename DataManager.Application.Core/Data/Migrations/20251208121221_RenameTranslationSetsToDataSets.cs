using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataManager.Application.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameTranslationSetsToDataSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename the tables to preserve existing data
            migrationBuilder.RenameTable(
                name: "TranslationSets",
                newName: "DataSets");

            migrationBuilder.RenameTable(
                name: "TranslationSetsIncludes",
                newName: "DataSetsIncludes");

            // Rename the column in Translations table
            migrationBuilder.RenameColumn(
                name: "TranslationSetId",
                table: "Translations",
                newName: "DataSetId");

            // Rename the index
            migrationBuilder.RenameIndex(
                name: "IX_Translations_TranslationSetId",
                table: "Translations",
                newName: "IX_Translations_DataSetId");

            // Rename columns in DataSetsIncludes table
            migrationBuilder.RenameColumn(
                name: "ParentTranslationSetId",
                table: "DataSetsIncludes",
                newName: "ParentDataSetId");

            migrationBuilder.RenameColumn(
                name: "IncludedTranslationSetId",
                table: "DataSetsIncludes",
                newName: "IncludedDataSetId");

            // Rename indexes in DataSetsIncludes table
            migrationBuilder.RenameIndex(
                name: "IX_TranslationSetsIncludes_IncludedTranslationSetId",
                table: "DataSetsIncludes",
                newName: "IX_DataSetsIncludes_IncludedDataSetId");

            migrationBuilder.RenameIndex(
                name: "IX_TranslationSetsIncludes_ParentTranslationSetId_IncludedTranslationSetId",
                table: "DataSetsIncludes",
                newName: "IX_DataSetsIncludes_ParentDataSetId_IncludedDataSetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse the renaming operations
            migrationBuilder.RenameTable(
                name: "DataSets",
                newName: "TranslationSets");

            migrationBuilder.RenameTable(
                name: "DataSetsIncludes",
                newName: "TranslationSetsIncludes");

            migrationBuilder.RenameColumn(
                name: "DataSetId",
                table: "Translations",
                newName: "TranslationSetId");

            migrationBuilder.RenameIndex(
                name: "IX_Translations_DataSetId",
                table: "Translations",
                newName: "IX_Translations_TranslationSetId");

            migrationBuilder.RenameColumn(
                name: "ParentDataSetId",
                table: "TranslationSetsIncludes",
                newName: "ParentTranslationSetId");

            migrationBuilder.RenameColumn(
                name: "IncludedDataSetId",
                table: "TranslationSetsIncludes",
                newName: "IncludedTranslationSetId");

            migrationBuilder.RenameIndex(
                name: "IX_DataSetsIncludes_IncludedDataSetId",
                table: "TranslationSetsIncludes",
                newName: "IX_TranslationSetsIncludes_IncludedTranslationSetId");

            migrationBuilder.RenameIndex(
                name: "IX_DataSetsIncludes_ParentDataSetId_IncludedDataSetId",
                table: "TranslationSetsIncludes",
                newName: "IX_TranslationSetsIncludes_ParentTranslationSetId_IncludedTranslationSetId");
        }
    }
}
