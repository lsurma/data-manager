using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataManager.Application.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    MainHost = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    ParentProjectId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectInstances_ProjectInstances_ParentProjectId",
                        column: x => x.ParentProjectId,
                        principalTable: "ProjectInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TranslationsSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    AllowedIdentityIds = table.Column<string>(type: "TEXT", nullable: false),
                    AvailableCultures = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranslationsSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Translations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InternalGroupName1 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    InternalGroupName2 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ResourceName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TranslationName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CultureName = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    ContentTemplate = table.Column<string>(type: "TEXT", nullable: true),
                    TranslationsSetId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SourceTranslationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SourceTranslationLastSyncedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LayoutId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SourceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsCurrentVersion = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    IsDraftVersion = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    IsOldVersion = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    OriginalTranslationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Translations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Translations_TranslationsSets_TranslationsSetId",
                        column: x => x.TranslationsSetId,
                        principalTable: "TranslationsSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Translations_Translations_LayoutId",
                        column: x => x.LayoutId,
                        principalTable: "Translations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Translations_Translations_OriginalTranslationId",
                        column: x => x.OriginalTranslationId,
                        principalTable: "Translations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Translations_Translations_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Translations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Translations_Translations_SourceTranslationId",
                        column: x => x.SourceTranslationId,
                        principalTable: "Translations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TranslationsSetsIncludes",
                columns: table => new
                {
                    ParentTranslationsSetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IncludedTranslationsSetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranslationsSetsIncludes", x => new { x.ParentTranslationsSetId, x.IncludedTranslationsSetId });
                    table.ForeignKey(
                        name: "FK_TranslationsSetsIncludes_TranslationsSets_IncludedTranslationsSetId",
                        column: x => x.IncludedTranslationsSetId,
                        principalTable: "TranslationsSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TranslationsSetsIncludes_TranslationsSets_ParentTranslationsSetId",
                        column: x => x.ParentTranslationsSetId,
                        principalTable: "TranslationsSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInstances_ParentProjectId",
                table: "ProjectInstances",
                column: "ParentProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Translations_CultureName",
                table: "Translations",
                column: "CultureName");

            migrationBuilder.CreateIndex(
                name: "IX_Translations_InternalGroupName1_InternalGroupName2_ResourceName_CultureName",
                table: "Translations",
                columns: new[] { "InternalGroupName1", "InternalGroupName2", "ResourceName", "CultureName" });

            migrationBuilder.CreateIndex(
                name: "IX_Translations_IsCurrentVersion_IsDraftVersion_IsOldVersion",
                table: "Translations",
                columns: new[] { "IsCurrentVersion", "IsDraftVersion", "IsOldVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_Translations_LayoutId",
                table: "Translations",
                column: "LayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_Translations_OriginalTranslationId",
                table: "Translations",
                column: "OriginalTranslationId");

            migrationBuilder.CreateIndex(
                name: "IX_Translations_SourceId",
                table: "Translations",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Translations_SourceTranslationId",
                table: "Translations",
                column: "SourceTranslationId");

            migrationBuilder.CreateIndex(
                name: "IX_Translations_TranslationsSetId",
                table: "Translations",
                column: "TranslationsSetId");

            migrationBuilder.CreateIndex(
                name: "IX_TranslationsSetsIncludes_IncludedTranslationsSetId",
                table: "TranslationsSetsIncludes",
                column: "IncludedTranslationsSetId");

            migrationBuilder.CreateIndex(
                name: "IX_TranslationsSetsIncludes_ParentTranslationsSetId_IncludedTranslationsSetId",
                table: "TranslationsSetsIncludes",
                columns: new[] { "ParentTranslationsSetId", "IncludedTranslationsSetId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectInstances");

            migrationBuilder.DropTable(
                name: "Translations");

            migrationBuilder.DropTable(
                name: "TranslationsSetsIncludes");

            migrationBuilder.DropTable(
                name: "TranslationsSets");
        }
    }
}
