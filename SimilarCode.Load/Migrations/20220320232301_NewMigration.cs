using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimilarCode.Load.Migrations
{
    public partial class NewMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Answers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CodeSnippetGroupings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnswerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeSnippetGroupings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodeSnippetGroupings_Answers_AnswerId",
                        column: x => x.AnswerId,
                        principalTable: "Answers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CodeSnippets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    CodeSnippetGroupingId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeSnippets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodeSnippets_CodeSnippetGroupings_CodeSnippetGroupingId",
                        column: x => x.CodeSnippetGroupingId,
                        principalTable: "CodeSnippetGroupings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgrammingLanguage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Language = table.Column<string>(type: "TEXT", nullable: false),
                    CodeSnippetId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgrammingLanguage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgrammingLanguage_CodeSnippets_CodeSnippetId",
                        column: x => x.CodeSnippetId,
                        principalTable: "CodeSnippets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CodeSnippetGroupings_AnswerId",
                table: "CodeSnippetGroupings",
                column: "AnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeSnippets_CodeSnippetGroupingId",
                table: "CodeSnippets",
                column: "CodeSnippetGroupingId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgrammingLanguage_CodeSnippetId",
                table: "ProgrammingLanguage",
                column: "CodeSnippetId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgrammingLanguage");

            migrationBuilder.DropTable(
                name: "CodeSnippets");

            migrationBuilder.DropTable(
                name: "CodeSnippetGroupings");

            migrationBuilder.DropTable(
                name: "Answers");
        }
    }
}
