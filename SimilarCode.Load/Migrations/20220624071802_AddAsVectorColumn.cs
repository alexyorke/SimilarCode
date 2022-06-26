using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimilarCode.Load.Migrations
{
    public partial class AddAsVectorColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentAsVector",
                table: "CodeSnippets",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentAsVector",
                table: "CodeSnippets");
        }
    }
}
