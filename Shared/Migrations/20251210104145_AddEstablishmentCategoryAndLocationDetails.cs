using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigBeerData.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddEstablishmentCategoryAndLocationDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Establishments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Establishments",
                keyColumn: "EstablishmentId",
                keyValue: 28351,
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                table: "Establishments",
                keyColumn: "EstablishmentId",
                keyValue: 1332593,
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                table: "Establishments",
                keyColumn: "EstablishmentId",
                keyValue: 1398768,
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                table: "Establishments",
                keyColumn: "EstablishmentId",
                keyValue: 2221333,
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                table: "Establishments",
                keyColumn: "EstablishmentId",
                keyValue: 3323017,
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                table: "Establishments",
                keyColumn: "EstablishmentId",
                keyValue: 3831964,
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                table: "Establishments",
                keyColumn: "EstablishmentId",
                keyValue: 4221968,
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                table: "Establishments",
                keyColumn: "EstablishmentId",
                keyValue: 4308714,
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                table: "Establishments",
                keyColumn: "EstablishmentId",
                keyValue: 6066172,
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                table: "Establishments",
                keyColumn: "EstablishmentId",
                keyValue: 7767345,
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                table: "Establishments",
                keyColumn: "EstablishmentId",
                keyValue: 7918310,
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                table: "Establishments",
                keyColumn: "EstablishmentId",
                keyValue: 9136453,
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "LocationId",
                keyValue: 1,
                columns: new[] { "Country", "State" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "LocationId",
                keyValue: 2,
                columns: new[] { "Country", "State" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "LocationId",
                keyValue: 3,
                columns: new[] { "Country", "State" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "LocationId",
                keyValue: 4,
                columns: new[] { "Country", "State" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Establishments");
        }
    }
}
