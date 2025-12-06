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
                name: "TranslationSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    AllowedIdentityIds = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranslationSets", x => x.Id);
                });

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
                name: "TranslationSetInclude",
                columns: table => new
                {
                    ParentTranslationSetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IncludedTranslationSetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranslationSetInclude", x => new { x.ParentTranslationSetId, x.IncludedTranslationSetId });
                    table.ForeignKey(
                        name: "FK_TranslationSetInclude_TranslationSets_IncludedTranslationSetId",
                        column: x => x.IncludedTranslationSetId,
                        principalTable: "TranslationSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TranslationSetInclude_TranslationSets_ParentTranslationSetId",
                        column: x => x.ParentTranslationSetId,
                        principalTable: "TranslationSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    TranslationSetId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LayoutId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SourceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Translations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Translations_TranslationSets_TranslationSetId",
                        column: x => x.TranslationSetId,
                        principalTable: "TranslationSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Translations_Translations_LayoutId",
                        column: x => x.LayoutId,
                        principalTable: "Translations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Translations_Translations_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Translations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TranslationSetInclude_IncludedTranslationSetId",
                table: "TranslationSetInclude",
                column: "IncludedTranslationSetId");

            migrationBuilder.CreateIndex(
                name: "IX_TranslationSetInclude_ParentTranslationSetId_IncludedTranslationSetId",
                table: "TranslationSetInclude",
                columns: new[] { "ParentTranslationSetId", "IncludedTranslationSetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInstances_ParentProjectId",
                table: "ProjectInstances",
                column: "ParentProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Translations_CultureName",
                table: "Translations",
                column: "CultureName");

            migrationBuilder.CreateIndex(
                name: "IX_Translations_TranslationSetId",
                table: "Translations",
                column: "TranslationSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Translations_InternalGroupName1_InternalGroupName2_ResourceName_CultureName",
                table: "Translations",
                columns: new[] { "InternalGroupName1", "InternalGroupName2", "ResourceName", "CultureName" });

            migrationBuilder.CreateIndex(
                name: "IX_Translations_LayoutId",
                table: "Translations",
                column: "LayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_Translations_SourceId",
                table: "Translations",
                column: "SourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TranslationSetInclude");

            migrationBuilder.DropTable(
                name: "ProjectInstances");

            migrationBuilder.DropTable(
                name: "Translations");

            migrationBuilder.DropTable(
                name: "TranslationSets");
        }
    }
}
