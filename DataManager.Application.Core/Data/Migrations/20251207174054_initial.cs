using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataManager.Application.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LogType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Target = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    StartedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    EndedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    Details = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
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
                name: "TranslationSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    AllowedIdentityIds = table.Column<string>(type: "TEXT", nullable: false),
                    AvailableCultures = table.Column<string>(type: "TEXT", nullable: false),
                    SecretKey = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    WebhookUrls = table.Column<string>(type: "TEXT", nullable: false),
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
                name: "Translations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InternalGroupName1 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    InternalGroupName2 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ResourceName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TranslationName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TranslationKey = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    CultureName = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    ContentTemplate = table.Column<string>(type: "TEXT", nullable: true),
                    ContentUpdatedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    TranslationSetId = table.Column<Guid>(type: "TEXT", nullable: true),
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
                name: "TranslationSetsIncludes",
                columns: table => new
                {
                    ParentTranslationSetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IncludedTranslationSetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranslationSetsIncludes", x => new { x.ParentTranslationSetId, x.IncludedTranslationSetId });
                    table.ForeignKey(
                        name: "FK_TranslationSetsIncludes_TranslationSets_IncludedTranslationSetId",
                        column: x => x.IncludedTranslationSetId,
                        principalTable: "TranslationSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TranslationSetsIncludes_TranslationSets_ParentTranslationSetId",
                        column: x => x.ParentTranslationSetId,
                        principalTable: "TranslationSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Logs_LogType",
                table: "Logs",
                column: "LogType");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_StartedAt",
                table: "Logs",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_Status",
                table: "Logs",
                column: "Status");

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
                name: "IX_Translations_TranslationKey",
                table: "Translations",
                column: "TranslationKey");

            migrationBuilder.CreateIndex(
                name: "IX_Translations_TranslationSetId",
                table: "Translations",
                column: "TranslationSetId");

            migrationBuilder.CreateIndex(
                name: "IX_TranslationSetsIncludes_IncludedTranslationSetId",
                table: "TranslationSetsIncludes",
                column: "IncludedTranslationSetId");

            migrationBuilder.CreateIndex(
                name: "IX_TranslationSetsIncludes_ParentTranslationSetId_IncludedTranslationSetId",
                table: "TranslationSetsIncludes",
                columns: new[] { "ParentTranslationSetId", "IncludedTranslationSetId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "ProjectInstances");

            migrationBuilder.DropTable(
                name: "Translations");

            migrationBuilder.DropTable(
                name: "TranslationSetsIncludes");

            migrationBuilder.DropTable(
                name: "TranslationSets");
        }
    }
}
