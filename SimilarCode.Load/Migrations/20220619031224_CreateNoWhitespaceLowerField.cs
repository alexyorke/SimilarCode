using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimilarCode.Load.Migrations
{
    public partial class CreateNoWhitespaceLowerField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentLowerNoWhitespace",
                table: "CodeSnippets",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentLowerNoWhitespace",
                table: "CodeSnippets");
        }
    }
}
