using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BigBeerData.Shared.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BeerColour",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false),
                    Hex = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeerColour", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "BeerType",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeerType", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "BeerYeast",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BaseColour = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeerYeast", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Brewers",
                columns: table => new
                {
                    BrewerId = table.Column<int>(type: "int", nullable: false),
                    SLUG = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BrewerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Lat = table.Column<double>(type: "float", nullable: false),
                    Long = table.Column<double>(type: "float", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    URL = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brewers", x => x.BrewerId);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    LocationName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.LocationId);
                });

            migrationBuilder.CreateTable(
                name: "BeerFamily",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeerTypeID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeerFamily", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BeerFamily_BeerType_BeerTypeID",
                        column: x => x.BeerTypeID,
                        principalTable: "BeerType",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Beers",
                columns: table => new
                {
                    Bid = table.Column<int>(type: "int", nullable: false),
                    SLUG = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeerPic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Style = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ABV = table.Column<double>(type: "float", nullable: false),
                    BrewerId = table.Column<int>(type: "int", nullable: false),
                    BaseStyle = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beers", x => x.Bid);
                    table.ForeignKey(
                        name: "FK_Beers_Brewers_BrewerId",
                        column: x => x.BrewerId,
                        principalTable: "Brewers",
                        principalColumn: "BrewerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Establishments",
                columns: table => new
                {
                    EstablishmentId = table.Column<int>(type: "int", nullable: false),
                    EstablishmentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Lat = table.Column<double>(type: "float", nullable: false),
                    Long = table.Column<double>(type: "float", nullable: false),
                    LastCheckinUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BaseZoom = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    MaxedCheckinHistory = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Establishments", x => x.EstablishmentId);
                    table.ForeignKey(
                        name: "FK_Establishments_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BeerStyle",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false),
                    FamilyID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypeID = table.Column<int>(type: "int", nullable: false),
                    ABVLow = table.Column<double>(type: "float", nullable: false),
                    ABVHigh = table.Column<double>(type: "float", nullable: false),
                    IBULow = table.Column<double>(type: "float", nullable: false),
                    IBUHigh = table.Column<double>(type: "float", nullable: false),
                    SRMLowID = table.Column<int>(type: "int", nullable: false),
                    SRMHighID = table.Column<int>(type: "int", nullable: false),
                    OrigGravLow = table.Column<double>(type: "float", nullable: false),
                    OrigGravHigh = table.Column<double>(type: "float", nullable: false),
                    FinalGravLow = table.Column<double>(type: "float", nullable: false),
                    FinalGravHigh = table.Column<double>(type: "float", nullable: false),
                    YeastID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeerStyle", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BeerStyle_BeerColour_SRMHighID",
                        column: x => x.SRMHighID,
                        principalTable: "BeerColour",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_BeerStyle_BeerColour_SRMLowID",
                        column: x => x.SRMLowID,
                        principalTable: "BeerColour",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_BeerStyle_BeerFamily_FamilyID",
                        column: x => x.FamilyID,
                        principalTable: "BeerFamily",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BeerStyle_BeerType_TypeID",
                        column: x => x.TypeID,
                        principalTable: "BeerType",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_BeerStyle_BeerYeast_YeastID",
                        column: x => x.YeastID,
                        principalTable: "BeerYeast",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Checkins",
                columns: table => new
                {
                    CheckinId = table.Column<double>(type: "float", nullable: false),
                    CheckinTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstablishmentId = table.Column<int>(type: "int", nullable: false),
                    Bid = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checkins", x => x.CheckinId);
                    table.ForeignKey(
                        name: "FK_Checkins_Beers_Bid",
                        column: x => x.Bid,
                        principalTable: "Beers",
                        principalColumn: "Bid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Checkins_Establishments_EstablishmentId",
                        column: x => x.EstablishmentId,
                        principalTable: "Establishments",
                        principalColumn: "EstablishmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "BeerColour",
                columns: new[] { "ID", "Hex" },
                values: new object[,]
                {
                    { 0, "#FFFFFF" },
                    { 23, "#892515" },
                    { 24, "#832212" },
                    { 26, "#771E0E" },
                    { 27, "#731C0B" },
                    { 28, "#70180C" },
                    { 29, "#6A160C" },
                    { 30, "#67120B" },
                    { 31, "#63100A" },
                    { 32, "#5F0E0A" },
                    { 33, "#5B0B0A" },
                    { 34, "#58080B" },
                    { 35, "#53080C" },
                    { 36, "#4B090B" },
                    { 37, "#470D0C" },
                    { 38, "#400C0E" },
                    { 39, "#3C0B0E" },
                    { 40, "#240A0B" },
                    { 22, "#8D2615" },
                    { 21, "#932A14" },
                    { 25, "#7D200F" },
                    { 19, "#9D3414" },
                    { 1, "#F8F4B4" },
                    { 2, "#F9E06C" },
                    { 3, "#F4CE51" },
                    { 4, "#F2BE37" },
                    { 20, "#983015" },
                    { 6, "#E59C19" },
                    { 7, "#DF8F16" },
                    { 8, "#D68019" },
                    { 9, "#CF731C" },
                    { 5, "#EDAC1E" },
                    { 11, "#C3621B" },
                    { 12, "#C86B1B" },
                    { 13, "#C05727" },
                    { 14, "#AD4417" },
                    { 15, "#AE4818" },
                    { 16, "#AD4417" },
                    { 17, "#A73D15" },
                    { 18, "#A23A15" },
                    { 10, "#BD591B" }
                });

            migrationBuilder.InsertData(
                table: "BeerType",
                columns: new[] { "ID", "Name" },
                values: new object[] { 3, "Mixed" });

            migrationBuilder.InsertData(
                table: "BeerType",
                columns: new[] { "ID", "Name" },
                values: new object[,]
                {
                    { 1, "Ale" },
                    { 2, "Lager" }
                });

            migrationBuilder.InsertData(
                table: "BeerYeast",
                columns: new[] { "ID", "BaseColour", "Name" },
                values: new object[,]
                {
                    { 1, "#775323", "Ale yeast with lactic bacteria" },
                    { 2, "#8E221F", "Wheat ale yeast" },
                    { 3, "#0A3D72", "Ale yeast" },
                    { 4, "#227547", "Lager yeast" }
                });

            migrationBuilder.InsertData(
                table: "Locations",
                columns: new[] { "LocationId", "LocationName" },
                values: new object[,]
                {
                    { 3, "Sydney" },
                    { 1, "Brisbane" },
                    { 2, "Melbourne" },
                    { 4, "Outer Brisbane" }
                });

            migrationBuilder.InsertData(
                table: "BeerFamily",
                columns: new[] { "ID", "BeerTypeID", "Name" },
                values: new object[,]
                {
                    { 1, 1, "Wheat Beer" },
                    { 20, 3, "Strong Ale" },
                    { 19, 3, "Barleywine" },
                    { 18, 3, "Smoked Beer" },
                    { 17, 3, "American Special" },
                    { 15, 3, "French Ale" },
                    { 14, 3, "Alt" },
                    { 13, 2, "Bock" },
                    { 12, 2, "European Lager" },
                    { 11, 2, "American Lager" },
                    { 16, 3, "German Amber Ale" },
                    { 9, 1, "Stout" },
                    { 8, 1, "Porter" },
                    { 7, 1, "Brown Ale" },
                    { 6, 1, "Scottish Ale" },
                    { 5, 1, "English Bitter" },
                    { 4, 1, "Pale Ale" },
                    { 3, 1, "Belgian Ale" },
                    { 2, 1, "Lambic and Sour Ale" },
                    { 10, 2, "Pilsner" }
                });

            migrationBuilder.InsertData(
                table: "Establishments",
                columns: new[] { "EstablishmentId", "BaseZoom", "EstablishmentName", "LastCheckinUpdate", "Lat", "LocationId", "Long", "MaxedCheckinHistory" },
                values: new object[,]
                {
                    { 1332593, 17, "Bitter Phew", null, -33.879899999999999, 3, 151.21600000000001, false },
                    { 7918310, 17, "Hellbound", null, -27.464500000000001, 1, 153.042, false },
                    { 7767345, 17, "Tippler's Tap", null, -27.457899999999999, 1, 153.042, false },
                    { 6066172, 17, "Malt Traders - Southbank", null, -27.479900000000001, 1, 153.023, false },
                    { 4221968, 17, "Tippler's Tap Southbank", null, -27.4801, 1, 153.023, false },
                    { 1398768, 15, "Brewski", null, -27.464500000000001, 1, 153.01300000000001, false },
                    { 3323017, 17, "Mr Edward's Alehouse", null, -27.471399999999999, 1, 153.03, false },
                    { 2221333, 17, "Malt Traders", null, -27.4697, 1, 153.03, false },
                    { 28351, 17, "Archive Beer Boutique", null, -27.479199999999999, 1, 153.01300000000001, false },
                    { 4308714, 18, "The Noble Hops", null, -33.892699999999998, 3, 151.202, false },
                    { 3831964, 17, "Saccharomyces Beer Cafe", null, -27.474499999999999, 1, 153.017, false },
                    { 9136453, 17, "The Woods Bar", null, -27.411999999999999, 4, 152.97499999999999, false }
                });

            migrationBuilder.InsertData(
                table: "BeerStyle",
                columns: new[] { "ID", "ABVHigh", "ABVLow", "FamilyID", "FinalGravHigh", "FinalGravLow", "IBUHigh", "IBULow", "Name", "OrigGravHigh", "OrigGravLow", "SRMHighID", "SRMLowID", "TypeID", "YeastID" },
                values: new object[,]
                {
                    { 1, 3.6000000000000001, 2.5, 1, 0.0, 0.0, 12.0, 3.0, "Berliner Weisse", 0.0, 0.0, 4, 2, 1, 1 },
                    { 22, 7.5, 5.0, 9, 0.0, 0.0, 70.0, 35.0, "Foreign Extra Stout", 0.0, 0.0, 40, 40, 1, 3 },
                    { 35, 6.4000000000000004, 3.2000000000000002, 9, 0.0, 0.0, 40.0, 20.0, "Sweet Stout", 0.0, 0.0, 40, 40, 1, 3 },
                    { 36, 9.0, 7.7999999999999998, 9, 0.0, 0.0, 80.0, 50.0, "Imperial Stout", 0.0, 0.0, 40, 40, 1, 3 },
                    { 49, 6.0999999999999996, 3.2999999999999998, 9, 0.0, 0.0, 50.0, 20.0, "Oatmeal Stout", 0.0, 0.0, 40, 40, 1, 3 },
                    { 50, 12.0, 8.0, 9, 0.0, 0.0, 90.0, 50.0, "Russian Imperial Stout", 0.0, 0.0, 40, 40, 1, 3 },
                    { 23, 5.4000000000000004, 4.5999999999999996, 10, 0.0, 0.0, 45.0, 25.0, "German Pilsner", 0.0, 0.0, 4, 2, 2, 4 },
                    { 37, 5.0999999999999996, 4.0999999999999996, 10, 0.0, 0.0, 45.0, 35.0, "Bohemian Pilsner", 0.0, 0.0, 5, 3, 2, 4 },
                    { 51, 6.0, 5.0, 10, 0.0, 0.0, 40.0, 20.0, "American Pilsner", 0.0, 0.0, 6, 3, 2, 4 },
                    { 11, 4.5, 2.8999999999999999, 11, 0.0, 0.0, 15.0, 8.0, "American Lite", 0.0, 0.0, 4, 2, 2, 4 },
                    { 24, 4.7999999999999998, 4.0999999999999996, 11, 0.0, 0.0, 17.0, 5.0, "American Standard", 0.0, 0.0, 6, 2, 2, 4 },
                    { 38, 5.0999999999999996, 4.5999999999999996, 11, 0.0, 0.0, 23.0, 13.0, "American Premium", 0.0, 0.0, 8, 2, 2, 4 },
                    { 52, 5.5999999999999996, 4.0999999999999996, 11, 0.0, 0.0, 20.0, 14.0, "American Dark", 0.0, 0.0, 20, 10, 2, 4 },
                    { 12, 5.5999999999999996, 4.5, 12, 0.0, 0.0, 25.0, 18.0, "Munich Helles", 0.0, 0.0, 5, 3, 2, 4 },
                    { 25, 6.0999999999999996, 5.0999999999999996, 12, 0.0, 0.0, 29.0, 23.0, "Dortmunder", 0.0, 0.0, 6, 4, 2, 4 },
                    { 39, 5.4000000000000004, 4.7999999999999998, 12, 0.0, 0.0, 25.0, 16.0, "Munich Dunkel", 0.0, 0.0, 23, 17, 2, 4 },
                    { 53, 5.0, 3.7999999999999998, 12, 0.0, 0.0, 30.0, 22.0, "Schwarzbier", 0.0, 0.0, 40, 25, 2, 4 },
                    { 13, 7.5, 6.0, 13, 0.0, 0.0, 35.0, 20.0, "Helles Bock", 0.0, 0.0, 10, 4, 2, 4 },
                    { 26, 7.9000000000000004, 6.5999999999999996, 13, 0.0, 0.0, 30.0, 20.0, "Doppelbock", 0.0, 0.0, 30, 12, 2, 4 },
                    { 40, 7.5999999999999996, 6.4000000000000004, 13, 0.0, 0.0, 30.0, 20.0, "Traditional Bock", 0.0, 0.0, 30, 15, 2, 4 },
                    { 54, 14.4, 8.6999999999999993, 13, 0.0, 0.0, 50.0, 25.0, "Eisbock", 0.0, 0.0, 40, 18, 2, 4 },
                    { 55, 5.2000000000000002, 4.7999999999999998, 14, 0.0, 0.0, 30.0, 20.0, "Kolsch", 0.0, 0.0, 5, 4, 3, 4 },
                    { 61, 5.0999999999999996, 4.5999999999999996, 14, 0.0, 0.0, 48.0, 25.0, "Altbier", 0.0, 0.0, 19, 11, 3, 4 },
                    { 56, 8.0, 4.5, 15, 0.0, 0.0, 30.0, 20.0, "Biere de garde", 0.0, 0.0, 12, 5, 3, 4 },
                    { 57, 6.5, 5.0999999999999996, 16, 0.0, 0.0, 30.0, 18.0, "Oktoberfest", 0.0, 0.0, 12, 7, 3, 3 },
                    { 62, 5.5, 4.5999999999999996, 16, 0.0, 0.0, 28.0, 20.0, "Vienna", 0.0, 0.0, 14, 8, 3, 3 },
                    { 58, 6.0, 4.5, 17, 0.0, 0.0, 35.0, 10.0, "Cream Ale", 0.0, 0.0, 14, 8, 3, 4 },
                    { 63, 5.0, 3.6000000000000001, 17, 0.0, 0.0, 45.0, 35.0, "Steam Beer", 0.0, 0.0, 17, 8, 3, 3 },
                    { 59, 5.5, 5.0, 18, 0.0, 0.0, 30.0, 20.0, "Smoked Beer", 0.0, 0.0, 17, 12, 3, 4 },
                    { 64, 12.199999999999999, 8.4000000000000004, 19, 0.0, 0.0, 100.0, 50.0, "Bareleywine", 0.0, 0.0, 22, 14, 3, 4 },
                    { 21, 5.5, 3.2000000000000002, 9, 0.0, 0.0, 50.0, 30.0, "Dry Stout", 0.0, 0.0, 40, 40, 1, 3 },
                    { 60, 8.5, 6.0999999999999996, 20, 0.0, 0.0, 40.0, 30.0, "English Old (Strong) Ale", 0.0, 0.0, 16, 12, 3, 4 },
                    { 48, 6.0, 4.7999999999999998, 8, 0.0, 0.0, 40.0, 30.0, "Robust Porter", 0.0, 0.0, 40, 30, 1, 3 },
                    { 47, 6.0, 3.5, 7, 0.0, 0.0, 25.0, 15.0, "English Brown", 0.0, 0.0, 30, 15, 1, 3 },
                    { 4, 5.5, 4.5, 1, 0.0, 0.0, 28.0, 15.0, "Belgian White", 0.0, 0.0, 4, 2, 1, 2 },
                    { 7, 5.0, 3.5, 1, 0.0, 0.0, 20.0, 5.0, "American Wheat", 0.0, 0.0, 8, 2, 1, 3 },
                    { 14, 5.5999999999999996, 4.2999999999999998, 1, 0.0, 0.0, 15.0, 8.0, "Weizenbier", 0.0, 0.0, 9, 3, 1, 2 },
                    { 27, 6.0, 4.5, 1, 0.0, 0.0, 15.0, 10.0, "Dunkelwiezen", 0.0, 0.0, 23, 17, 1, 2 },
                    { 41, 9.5999999999999996, 6.5, 1, 0.0, 0.0, 25.0, 15.0, "Wiezenbock", 0.0, 0.0, 30, 10, 1, 2 },
                    { 2, 6.4000000000000004, 4.7000000000000002, 2, 0.0, 0.0, 15.0, 5.0, "Lambic", 0.0, 0.0, 15, 4, 1, 1 },
                    { 5, 6.4000000000000004, 4.7000000000000002, 2, 0.0, 0.0, 15.0, 5.0, "Gueuze", 0.0, 0.0, 15, 4, 1, 1 },
                    { 8, 5.5, 4.5, 2, 0.0, 0.0, 15.0, 5.0, "Faro", 0.0, 0.0, 15, 4, 1, 1 }
                });

            migrationBuilder.InsertData(
                table: "BeerStyle",
                columns: new[] { "ID", "ABVHigh", "ABVLow", "FamilyID", "FinalGravHigh", "FinalGravLow", "IBUHigh", "IBULow", "Name", "OrigGravHigh", "OrigGravLow", "SRMHighID", "SRMLowID", "TypeID", "YeastID" },
                values: new object[,]
                {
                    { 15, 7.0, 4.7000000000000002, 2, 0.0, 0.0, 21.0, 15.0, "Fruit Beer", 0.0, 0.0, 0, 0, 1, 1 },
                    { 28, 5.7999999999999998, 4.0, 2, 0.0, 0.0, 25.0, 14.0, "Flanders red", 0.0, 0.0, 16, 10, 1, 1 },
                    { 42, 6.5, 4.0, 2, 0.0, 0.0, 30.0, 14.0, "Oud bruin", 0.0, 0.0, 20, 12, 1, 1 },
                    { 3, 9.0, 7.0, 3, 0.0, 0.0, 35.0, 25.0, "Belgian Gold Ale", 0.0, 0.0, 6, 4, 1, 3 },
                    { 6, 10.0, 7.0, 3, 0.0, 0.0, 30.0, 20.0, "Tripel", 0.0, 0.0, 7, 4, 1, 3 },
                    { 9, 8.0999999999999996, 4.5, 3, 0.0, 0.0, 40.0, 25.0, "Saison", 0.0, 0.0, 10, 4, 1, 3 },
                    { 16, 5.5999999999999996, 3.8999999999999999, 3, 0.0, 0.0, 35.0, 20.0, "Belgian Pale Ale", 0.0, 0.0, 14, 4, 1, 3 },
                    { 29, 12.0, 7.0, 3, 0.0, 0.0, 40.0, 25.0, "Belgian Dark Ale", 0.0, 0.0, 20, 7, 1, 3 },
                    { 43, 8.0, 3.2000000000000002, 3, 0.0, 0.0, 25.0, 20.0, "Dubbel", 0.0, 0.0, 20, 10, 1, 3 },
                    { 10, 5.5, 4.5, 4, 0.0, 0.0, 40.0, 20.0, "Pale Ale", 0.0, 0.0, 11, 4, 1, 3 },
                    { 17, 5.7000000000000002, 4.5, 4, 0.0, 0.0, 40.0, 20.0, "American Pale Ale", 0.0, 0.0, 11, 4, 1, 3 },
                    { 30, 7.5999999999999996, 5.0999999999999996, 4, 0.0, 0.0, 60.0, 40.0, "India Pale Ale", 0.0, 0.0, 14, 8, 1, 3 },
                    { 44, 5.7000000000000002, 4.5, 4, 0.0, 0.0, 40.0, 20.0, "American Amber Ale", 0.0, 0.0, 18, 11, 1, 3 },
                    { 18, 3.7999999999999998, 3.0, 5, 0.0, 0.0, 335.0, 20.0, "Ordinary Bitter", 0.0, 0.0, 12, 6, 1, 3 },
                    { 31, 4.7999999999999998, 3.7000000000000002, 5, 0.0, 0.0, 40.0, 25.0, "Special Bitter", 0.0, 0.0, 14, 12, 1, 3 },
                    { 45, 4.5999999999999996, 3.7000000000000002, 5, 0.0, 0.0, 45.0, 30.0, "Extraspecial Bitter", 0.0, 0.0, 14, 12, 1, 3 },
                    { 19, 4.0, 2.7999999999999998, 6, 0.0, 0.0, 20.0, 9.0, "Scottish Light 60/-", 0.0, 0.0, 17, 8, 1, 3 },
                    { 32, 4.0999999999999996, 3.5, 6, 0.0, 0.0, 25.0, 12.0, "Scottish Heavy 70/-", 0.0, 0.0, 19, 10, 1, 3 },
                    { 46, 4.9000000000000004, 4.0, 6, 0.0, 0.0, 36.0, 15.0, "Scottish Export 80/-", 0.0, 0.0, 19, 10, 1, 3 },
                    { 20, 4.0999999999999996, 2.5, 7, 0.0, 0.0, 24.0, 10.0, "English Mid", 0.0, 0.0, 25, 10, 1, 3 },
                    { 33, 6.0, 4.2000000000000002, 7, 0.0, 0.0, 60.0, 25.0, "American Brown", 0.0, 0.0, 22, 15, 1, 3 },
                    { 34, 5.2000000000000002, 3.7999999999999998, 8, 0.0, 0.0, 35.0, 20.0, "Brown Porter", 0.0, 0.0, 35, 20, 1, 3 },
                    { 65, 9.0, 6.0, 20, 0.0, 0.0, 40.0, 20.0, "Strong \"Scotch\" Ale", 0.0, 0.0, 40, 10, 3, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BeerFamily_BeerTypeID",
                table: "BeerFamily",
                column: "BeerTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Beers_BrewerId",
                table: "Beers",
                column: "BrewerId");

            migrationBuilder.CreateIndex(
                name: "IX_BeerStyle_FamilyID",
                table: "BeerStyle",
                column: "FamilyID");

            migrationBuilder.CreateIndex(
                name: "IX_BeerStyle_SRMHighID",
                table: "BeerStyle",
                column: "SRMHighID");

            migrationBuilder.CreateIndex(
                name: "IX_BeerStyle_SRMLowID",
                table: "BeerStyle",
                column: "SRMLowID");

            migrationBuilder.CreateIndex(
                name: "IX_BeerStyle_TypeID",
                table: "BeerStyle",
                column: "TypeID");

            migrationBuilder.CreateIndex(
                name: "IX_BeerStyle_YeastID",
                table: "BeerStyle",
                column: "YeastID");

            migrationBuilder.CreateIndex(
                name: "IX_Checkins_Bid",
                table: "Checkins",
                column: "Bid");

            migrationBuilder.CreateIndex(
                name: "IX_Checkins_EstablishmentId",
                table: "Checkins",
                column: "EstablishmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Establishments_LocationId",
                table: "Establishments",
                column: "LocationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeerStyle");

            migrationBuilder.DropTable(
                name: "Checkins");

            migrationBuilder.DropTable(
                name: "BeerColour");

            migrationBuilder.DropTable(
                name: "BeerFamily");

            migrationBuilder.DropTable(
                name: "BeerYeast");

            migrationBuilder.DropTable(
                name: "Beers");

            migrationBuilder.DropTable(
                name: "Establishments");

            migrationBuilder.DropTable(
                name: "BeerType");

            migrationBuilder.DropTable(
                name: "Brewers");

            migrationBuilder.DropTable(
                name: "Locations");
        }
    }
}
