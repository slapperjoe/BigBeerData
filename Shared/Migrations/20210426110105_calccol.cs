using Microsoft.EntityFrameworkCore.Migrations;

namespace BigBeerData.Shared.Migrations
{
    public partial class calccol : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "BaseStyle",
                table: "Beers",
                type: "nvarchar(max)",
                nullable: true,
                computedColumnSql: "(case when charindex(' - ',[Style])=(0) then [Style] else rtrim(left([Style],charindex(' - ',[Style]))) end)",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "BaseStyle",
                table: "Beers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComputedColumnSql: "(case when charindex(' - ',[Style])=(0) then [Style] else rtrim(left([Style],charindex(' - ',[Style]))) end)");
        }
    }
}
