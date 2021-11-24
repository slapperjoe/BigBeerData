using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigBeerData.Shared
{
	public partial class BigBeerContext
	{
		public static void LoadData(ModelBuilder modelBuilder) {
			modelBuilder.Entity<Location>().HasData(
					new Location { LocationId = 1, LocationName = "Brisbane" },
					new Location { LocationId = 2, LocationName = "Melbourne" },
					new Location { LocationId = 3, LocationName = "Sydney" },
					new Location { LocationId = 4, LocationName = "Outer Brisbane" });
			modelBuilder.Entity<BeerType>().HasData(
				new BeerType { ID = 1, Name = "Ale" },
				new BeerType { ID = 2, Name = "Lager" },
				new BeerType { ID = 3, Name = "Mixed" });
			modelBuilder.Entity<BeerFamily>().HasData(new BeerFamily { ID = 1, Name = "Wheat Beer", BeerTypeID = 1 },
				new BeerFamily { ID = 2, Name = "Lambic and Sour Ale", BeerTypeID = 1 },
				new BeerFamily { ID = 3, Name = "Belgian Ale", BeerTypeID = 1 },
				new BeerFamily { ID = 4, Name = "Pale Ale", BeerTypeID = 1 },
				new BeerFamily { ID = 5, Name = "English Bitter", BeerTypeID = 1 },
				new BeerFamily { ID = 6, Name = "Scottish Ale", BeerTypeID = 1 },
				new BeerFamily { ID = 7, Name = "Brown Ale", BeerTypeID = 1 },
				new BeerFamily { ID = 8, Name = "Porter", BeerTypeID = 1 },
				new BeerFamily { ID = 9, Name = "Stout", BeerTypeID = 1 },
				new BeerFamily { ID = 10, Name = "Pilsner", BeerTypeID = 2 },
				new BeerFamily { ID = 11, Name = "American Lager", BeerTypeID = 2 },
				new BeerFamily { ID = 12, Name = "European Lager", BeerTypeID = 2 },
				new BeerFamily { ID = 13, Name = "Bock", BeerTypeID = 2 },
				new BeerFamily { ID = 14, Name = "Alt", BeerTypeID = 3 },
				new BeerFamily { ID = 15, Name = "French Ale", BeerTypeID = 3 },
				new BeerFamily { ID = 16, Name = "German Amber Ale", BeerTypeID = 3 },
				new BeerFamily { ID = 17, Name = "American Special", BeerTypeID = 3 },
				new BeerFamily { ID = 18, Name = "Smoked Beer", BeerTypeID = 3 },
				new BeerFamily { ID = 19, Name = "Barleywine", BeerTypeID = 3 },
				new BeerFamily { ID = 20, Name = "Strong Ale", BeerTypeID = 3 });


			modelBuilder.Entity<BeerStyle>()
				.HasData(
				new BeerStyle { ID = 1, FamilyID = 1, Name = "Berliner Weisse", TypeID = 1, ABVLow = 2.5, ABVHigh = 3.6, IBULow = 3, IBUHigh = 12, SRMLowID = 2, SRMHighID = 4, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 1 },
				new BeerStyle { ID = 2, FamilyID = 2, Name = "Lambic", TypeID = 1, ABVLow = 4.7, ABVHigh = 6.4, IBULow = 5, IBUHigh = 15, SRMLowID = 4, SRMHighID = 15, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 1 },
				new BeerStyle { ID = 3, FamilyID = 3, Name = "Belgian Gold Ale", TypeID = 1, ABVLow = 7, ABVHigh = 9, IBULow = 25, IBUHigh = 35, SRMLowID = 4, SRMHighID = 6, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 4, FamilyID = 1, Name = "Belgian White", TypeID = 1, ABVLow = 4.5, ABVHigh = 5.5, IBULow = 15, IBUHigh = 28, SRMLowID = 2, SRMHighID = 4, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 2 },
				new BeerStyle { ID = 5, FamilyID = 2, Name = "Gueuze", TypeID = 1, ABVLow = 4.7, ABVHigh = 6.4, IBULow = 5, IBUHigh = 15, SRMLowID = 4, SRMHighID = 15, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 1 },
				new BeerStyle { ID = 6, FamilyID = 3, Name = "Tripel", TypeID = 1, ABVLow = 7, ABVHigh = 10, IBULow = 20, IBUHigh = 30, SRMLowID = 4, SRMHighID = 7, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 7, FamilyID = 1, Name = "American Wheat", TypeID = 1, ABVLow = 3.5, ABVHigh = 5, IBULow = 5, IBUHigh = 20, SRMLowID = 2, SRMHighID = 8, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 8, FamilyID = 2, Name = "Faro", TypeID = 1, ABVLow = 4.5, ABVHigh = 5.5, IBULow = 5, IBUHigh = 15, SRMLowID = 4, SRMHighID = 15, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 1 },
				new BeerStyle { ID = 9, FamilyID = 3, Name = "Saison", TypeID = 1, ABVLow = 4.5, ABVHigh = 8.1, IBULow = 25, IBUHigh = 40, SRMLowID = 4, SRMHighID = 10, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 10, FamilyID = 4, Name = "Pale Ale", TypeID = 1, ABVLow = 4.5, ABVHigh = 5.5, IBULow = 20, IBUHigh = 40, SRMLowID = 4, SRMHighID = 11, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 11, FamilyID = 11, Name = "American Lite", TypeID = 2, ABVLow = 2.9, ABVHigh = 4.5, IBULow = 8, IBUHigh = 15, SRMLowID = 2, SRMHighID = 4, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 4 },
				new BeerStyle { ID = 12, FamilyID = 12, Name = "Munich Helles", TypeID = 2, ABVLow = 4.5, ABVHigh = 5.6, IBULow = 18, IBUHigh = 25, SRMLowID = 3, SRMHighID = 5, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 4 },
				new BeerStyle { ID = 13, FamilyID = 13, Name = "Helles Bock", TypeID = 2, ABVLow = 6, ABVHigh = 7.5, IBULow = 20, IBUHigh = 35, SRMLowID = 4, SRMHighID = 10, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 4 },
				new BeerStyle { ID = 14, FamilyID = 1, Name = "Weizenbier", TypeID = 1, ABVLow = 4.3, ABVHigh = 5.6, IBULow = 8, IBUHigh = 15, SRMLowID = 3, SRMHighID = 9, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 2 },
				new BeerStyle { ID = 15, FamilyID = 2, Name = "Fruit Beer", TypeID = 1, ABVLow = 4.7, ABVHigh = 7, IBULow = 15, IBUHigh = 21, SRMLowID = 0, SRMHighID = 0, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 1 },
				new BeerStyle { ID = 16, FamilyID = 3, Name = "Belgian Pale Ale", TypeID = 1, ABVLow = 3.9, ABVHigh = 5.6, IBULow = 20, IBUHigh = 35, SRMLowID = 4, SRMHighID = 14, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 17, FamilyID = 4, Name = "American Pale Ale", TypeID = 1, ABVLow = 4.5, ABVHigh = 5.7, IBULow = 20, IBUHigh = 40, SRMLowID = 4, SRMHighID = 11, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 18, FamilyID = 5, Name = "Ordinary Bitter", TypeID = 1, ABVLow = 3, ABVHigh = 3.8, IBULow = 20, IBUHigh = 335, SRMLowID = 6, SRMHighID = 12, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 19, FamilyID = 6, Name = "Scottish Light 60/-", TypeID = 1, ABVLow = 2.8, ABVHigh = 4, IBULow = 9, IBUHigh = 20, SRMLowID = 8, SRMHighID = 17, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 20, FamilyID = 7, Name = "English Mid", TypeID = 1, ABVLow = 2.5, ABVHigh = 4.1, IBULow = 10, IBUHigh = 24, SRMLowID = 10, SRMHighID = 25, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 21, FamilyID = 9, Name = "Dry Stout", TypeID = 1, ABVLow = 3.2, ABVHigh = 5.5, IBULow = 30, IBUHigh = 50, SRMLowID = 40, SRMHighID = 40, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 22, FamilyID = 9, Name = "Foreign Extra Stout", TypeID = 1, ABVLow = 5, ABVHigh = 7.5, IBULow = 35, IBUHigh = 70, SRMLowID = 40, SRMHighID = 40, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 23, FamilyID = 10, Name = "German Pilsner", TypeID = 2, ABVLow = 4.6, ABVHigh = 5.4, IBULow = 25, IBUHigh = 45, SRMLowID = 2, SRMHighID = 4, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 4 },
				new BeerStyle { ID = 24, FamilyID = 11, Name = "American Standard", TypeID = 2, ABVLow = 4.1, ABVHigh = 4.8, IBULow = 5, IBUHigh = 17, SRMLowID = 2, SRMHighID = 6, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 4 },
				new BeerStyle { ID = 25, FamilyID = 12, Name = "Dortmunder", TypeID = 2, ABVLow = 5.1, ABVHigh = 6.1, IBULow = 23, IBUHigh = 29, SRMLowID = 4, SRMHighID = 6, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 4 },
				new BeerStyle { ID = 26, FamilyID = 13, Name = "Doppelbock", TypeID = 2, ABVLow = 6.6, ABVHigh = 7.9, IBULow = 20, IBUHigh = 30, SRMLowID = 12, SRMHighID = 30, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 4 },
				new BeerStyle { ID = 27, FamilyID = 1, Name = "Dunkelwiezen", TypeID = 1, ABVLow = 4.5, ABVHigh = 6, IBULow = 10, IBUHigh = 15, SRMLowID = 17, SRMHighID = 23, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 2 },
				new BeerStyle { ID = 28, FamilyID = 2, Name = "Flanders red", TypeID = 1, ABVLow = 4, ABVHigh = 5.8, IBULow = 14, IBUHigh = 25, SRMLowID = 10, SRMHighID = 16, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 1 },
				new BeerStyle { ID = 29, FamilyID = 3, Name = "Belgian Dark Ale", TypeID = 1, ABVLow = 7, ABVHigh = 12, IBULow = 25, IBUHigh = 40, SRMLowID = 7, SRMHighID = 20, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 30, FamilyID = 4, Name = "India Pale Ale", TypeID = 1, ABVLow = 5.1, ABVHigh = 7.6, IBULow = 40, IBUHigh = 60, SRMLowID = 8, SRMHighID = 14, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 31, FamilyID = 5, Name = "Special Bitter", TypeID = 1, ABVLow = 3.7, ABVHigh = 4.8, IBULow = 25, IBUHigh = 40, SRMLowID = 12, SRMHighID = 14, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 32, FamilyID = 6, Name = "Scottish Heavy 70/-", TypeID = 1, ABVLow = 3.5, ABVHigh = 4.1, IBULow = 12, IBUHigh = 25, SRMLowID = 10, SRMHighID = 19, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 33, FamilyID = 7, Name = "American Brown", TypeID = 1, ABVLow = 4.2, ABVHigh = 6, IBULow = 25, IBUHigh = 60, SRMLowID = 15, SRMHighID = 22, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 34, FamilyID = 8, Name = "Brown Porter", TypeID = 1, ABVLow = 3.8, ABVHigh = 5.2, IBULow = 20, IBUHigh = 35, SRMLowID = 20, SRMHighID = 35, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 35, FamilyID = 9, Name = "Sweet Stout", TypeID = 1, ABVLow = 3.2, ABVHigh = 6.4, IBULow = 20, IBUHigh = 40, SRMLowID = 40, SRMHighID = 40, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 36, FamilyID = 9, Name = "Imperial Stout", TypeID = 1, ABVLow = 7.8, ABVHigh = 9, IBULow = 50, IBUHigh = 80, SRMLowID = 40, SRMHighID = 40, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 37, FamilyID = 10, Name = "Bohemian Pilsner", TypeID = 2, ABVLow = 4.1, ABVHigh = 5.1, IBULow = 35, IBUHigh = 45, SRMLowID = 3, SRMHighID = 5, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 4 },
				new BeerStyle { ID = 38, FamilyID = 11, Name = "American Premium", TypeID = 2, ABVLow = 4.6, ABVHigh = 5.1, IBULow = 13, IBUHigh = 23, SRMLowID = 2, SRMHighID = 8, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 4 },
				new BeerStyle { ID = 39, FamilyID = 12, Name = "Munich Dunkel", TypeID = 2, ABVLow = 4.8, ABVHigh = 5.4, IBULow = 16, IBUHigh = 25, SRMLowID = 17, SRMHighID = 23, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 4 },
				new BeerStyle { ID = 40, FamilyID = 13, Name = "Traditional Bock", TypeID = 2, ABVLow = 6.4, ABVHigh = 7.6, IBULow = 20, IBUHigh = 30, SRMLowID = 15, SRMHighID = 30, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 4 },
				new BeerStyle { ID = 41, FamilyID = 1, Name = "Wiezenbock", TypeID = 1, ABVLow = 6.5, ABVHigh = 9.6, IBULow = 15, IBUHigh = 25, SRMLowID = 10, SRMHighID = 30, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 2 },
				new BeerStyle { ID = 42, FamilyID = 2, Name = "Oud bruin", TypeID = 1, ABVLow = 4, ABVHigh = 6.5, IBULow = 14, IBUHigh = 30, SRMLowID = 12, SRMHighID = 20, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 1 },
				new BeerStyle { ID = 43, FamilyID = 3, Name = "Dubbel", TypeID = 1, ABVLow = 3.2, ABVHigh = 8, IBULow = 20, IBUHigh = 25, SRMLowID = 10, SRMHighID = 20, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 44, FamilyID = 4, Name = "American Amber Ale", TypeID = 1, ABVLow = 4.5, ABVHigh = 5.7, IBULow = 20, IBUHigh = 40, SRMLowID = 11, SRMHighID = 18, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 45, FamilyID = 5, Name = "Extraspecial Bitter", TypeID = 1, ABVLow = 3.7, ABVHigh = 4.6, IBULow = 30, IBUHigh = 45, SRMLowID = 12, SRMHighID = 14, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 46, FamilyID = 6, Name = "Scottish Export 80/-", TypeID = 1, ABVLow = 4, ABVHigh = 4.9, IBULow = 15, IBUHigh = 36, SRMLowID = 10, SRMHighID = 19, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 47, FamilyID = 7, Name = "English Brown", TypeID = 1, ABVLow = 3.5, ABVHigh = 6, IBULow = 15, IBUHigh = 25, SRMLowID = 15, SRMHighID = 30, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 48, FamilyID = 8, Name = "Robust Porter", TypeID = 1, ABVLow = 4.8, ABVHigh = 6, IBULow = 30, IBUHigh = 40, SRMLowID = 30, SRMHighID = 40, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 49, FamilyID = 9, Name = "Oatmeal Stout", TypeID = 1, ABVLow = 3.3, ABVHigh = 6.1, IBULow = 20, IBUHigh = 50, SRMLowID = 40, SRMHighID = 40, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 50, FamilyID = 9, Name = "Russian Imperial Stout", TypeID = 1, ABVLow = 8, ABVHigh = 12, IBULow = 50, IBUHigh = 90, SRMLowID = 40, SRMHighID = 40, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 51, FamilyID = 10, Name = "American Pilsner", TypeID = 2, ABVLow = 5, ABVHigh = 6, IBULow = 20, IBUHigh = 40, SRMLowID = 3, SRMHighID = 6, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 4 },
				new BeerStyle { ID = 52, FamilyID = 11, Name = "American Dark", TypeID = 2, ABVLow = 4.1, ABVHigh = 5.6, IBULow = 14, IBUHigh = 20, SRMLowID = 10, SRMHighID = 20, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 4 },
				new BeerStyle { ID = 53, FamilyID = 12, Name = "Schwarzbier", TypeID = 2, ABVLow = 3.8, ABVHigh = 5, IBULow = 22, IBUHigh = 30, SRMLowID = 25, SRMHighID = 40, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 4 },
				new BeerStyle { ID = 54, FamilyID = 13, Name = "Eisbock", TypeID = 2, ABVLow = 8.7, ABVHigh = 14.4, IBULow = 25, IBUHigh = 50, SRMLowID = 18, SRMHighID = 40, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 4 },
				new BeerStyle { ID = 55, FamilyID = 14, Name = "Kolsch", TypeID = 3, ABVLow = 4.8, ABVHigh = 5.2, IBULow = 20, IBUHigh = 30, SRMLowID = 4, SRMHighID = 5, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 4 },
				new BeerStyle { ID = 56, FamilyID = 15, Name = "Biere de garde", TypeID = 3, ABVLow = 4.5, ABVHigh = 8, IBULow = 20, IBUHigh = 30, SRMLowID = 5, SRMHighID = 12, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 4 },
				new BeerStyle { ID = 57, FamilyID = 16, Name = "Oktoberfest", TypeID = 3, ABVLow = 5.1, ABVHigh = 6.5, IBULow = 18, IBUHigh = 30, SRMLowID = 7, SRMHighID = 12, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 58, FamilyID = 17, Name = "Cream Ale", TypeID = 3, ABVLow = 4.5, ABVHigh = 6, IBULow = 10, IBUHigh = 35, SRMLowID = 8, SRMHighID = 14, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 4 },
				new BeerStyle { ID = 59, FamilyID = 18, Name = "Smoked Beer", TypeID = 3, ABVLow = 5, ABVHigh = 5.5, IBULow = 20, IBUHigh = 30, SRMLowID = 12, SRMHighID = 17, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 4 },
				new BeerStyle { ID = 60, FamilyID = 20, Name = "English Old (Strong) Ale", TypeID = 3, ABVLow = 6.1, ABVHigh = 8.5, IBULow = 30, IBUHigh = 40, SRMLowID = 12, SRMHighID = 16, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 4 },
				new BeerStyle { ID = 61, FamilyID = 14, Name = "Altbier", TypeID = 3, ABVLow = 4.6, ABVHigh = 5.1, IBULow = 25, IBUHigh = 48, SRMLowID = 11, SRMHighID = 19, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 4 },
				new BeerStyle { ID = 62, FamilyID = 16, Name = "Vienna", TypeID = 3, ABVLow = 4.6, ABVHigh = 5.5, IBULow = 20, IBUHigh = 28, SRMLowID = 8, SRMHighID = 14, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 63, FamilyID = 17, Name = "Steam Beer", TypeID = 3, ABVLow = 3.6, ABVHigh = 5, IBULow = 35, IBUHigh = 45, SRMLowID = 8, SRMHighID = 17, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 3 },
				new BeerStyle { ID = 64, FamilyID = 19, Name = "Bareleywine", TypeID = 3, ABVLow = 8.4, ABVHigh = 12.2, IBULow = 50, IBUHigh = 100, SRMLowID = 14, SRMHighID = 22, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 4 },
				new BeerStyle { ID = 65, FamilyID = 20, Name = "Strong \"Scotch\" Ale", TypeID = 3, ABVLow = 6, ABVHigh = 9, IBULow = 20, IBUHigh = 40, SRMLowID = 10, SRMHighID = 40, OrigGravLow = 0, OrigGravHigh = 0, FinalGravLow = 0, FinalGravHigh = 0, YeastID = 4 });
			modelBuilder.Entity<BeerColour>().HasData(new BeerColour { ID = 0, Hex = "#FFFFFF" },
				new BeerColour { ID = 1, Hex = "#F8F4B4" },
				new BeerColour { ID = 2, Hex = "#F9E06C" },
				new BeerColour { ID = 3, Hex = "#F4CE51" },
				new BeerColour { ID = 4, Hex = "#F2BE37" },
				new BeerColour { ID = 5, Hex = "#EDAC1E" },
				new BeerColour { ID = 6, Hex = "#E59C19" },
				new BeerColour { ID = 7, Hex = "#DF8F16" },
				new BeerColour { ID = 8, Hex = "#D68019" },
				new BeerColour { ID = 9, Hex = "#CF731C" },
				new BeerColour { ID = 10, Hex = "#BD591B" },
				new BeerColour { ID = 11, Hex = "#C3621B" },
				new BeerColour { ID = 12, Hex = "#C86B1B" },
				new BeerColour { ID = 13, Hex = "#C05727" },
				new BeerColour { ID = 14, Hex = "#AD4417" },
				new BeerColour { ID = 15, Hex = "#AE4818" },
				new BeerColour { ID = 16, Hex = "#AD4417" },
				new BeerColour { ID = 17, Hex = "#A73D15" },
				new BeerColour { ID = 18, Hex = "#A23A15" },
				new BeerColour { ID = 19, Hex = "#9D3414" },
				new BeerColour { ID = 20, Hex = "#983015" },
				new BeerColour { ID = 21, Hex = "#932A14" },
				new BeerColour { ID = 22, Hex = "#8D2615" },
				new BeerColour { ID = 23, Hex = "#892515" },
				new BeerColour { ID = 24, Hex = "#832212" },
				new BeerColour { ID = 25, Hex = "#7D200F" },
				new BeerColour { ID = 26, Hex = "#771E0E" },
				new BeerColour { ID = 27, Hex = "#731C0B" },
				new BeerColour { ID = 28, Hex = "#70180C" },
				new BeerColour { ID = 29, Hex = "#6A160C" },
				new BeerColour { ID = 30, Hex = "#67120B" },
				new BeerColour { ID = 31, Hex = "#63100A" },
				new BeerColour { ID = 32, Hex = "#5F0E0A" },
				new BeerColour { ID = 33, Hex = "#5B0B0A" },
				new BeerColour { ID = 34, Hex = "#58080B" },
				new BeerColour { ID = 35, Hex = "#53080C" },
				new BeerColour { ID = 36, Hex = "#4B090B" },
				new BeerColour { ID = 37, Hex = "#470D0C" },
				new BeerColour { ID = 38, Hex = "#400C0E" },
				new BeerColour { ID = 39, Hex = "#3C0B0E" },
				new BeerColour { ID = 40, Hex = "#240A0B" });
			modelBuilder.Entity<BeerYeast>().HasData(
				new BeerYeast { ID = 1, Name = "Ale yeast with lactic bacteria", BaseColour = "#775323" },
				new BeerYeast { ID = 2, Name = "Wheat ale yeast", BaseColour = "#8E221F" },
				new BeerYeast { ID = 3, Name = "Ale yeast", BaseColour = "#0A3D72" },
				new BeerYeast { ID = 4, Name = "Lager yeast", BaseColour = "#227547" });
			modelBuilder.Entity<Establishment>().HasData(
				new Establishment
				{
					EstablishmentId = 28351,
					EstablishmentName = "Archive Beer Boutique",
					Lat = -27.4792,
					Long = 153.013,
					BaseZoom = 17,
					LocationId = 1
				},
				new Establishment
				{
					EstablishmentId = 1332593,
					EstablishmentName = "Bitter Phew",
					Lat = -33.8799,
					Long = 151.216,
					BaseZoom = 17,
					LocationId = 3
				},
				new Establishment
				{
					EstablishmentId = 1398768,
					EstablishmentName = "Brewski",
					Lat = -27.4645,
					Long = 153.013,
					BaseZoom = 15,
					LocationId = 1
				},
				new Establishment
				{
					EstablishmentId = 2221333,
					EstablishmentName = "Malt Traders",
					Lat = -27.4697,
					Long = 153.03,
					BaseZoom = 17,
					LocationId = 1
				},
				new Establishment
				{
					EstablishmentId = 3323017,
					EstablishmentName = "Mr Edward's Alehouse",
					Lat = -27.4714,
					Long = 153.03,
					BaseZoom = 17,
					LocationId = 1
				},
				new Establishment
				{
					EstablishmentId = 3831964,
					EstablishmentName = "Saccharomyces Beer Cafe",
					Lat = -27.4745,
					Long = 153.017,
					BaseZoom = 17,
					LocationId = 1
				},
				new Establishment
				{
					EstablishmentId = 4221968,
					EstablishmentName = "Tippler's Tap Southbank",
					Lat = -27.4801,
					Long = 153.023,
					BaseZoom = 17,
					LocationId = 1
				},
				new Establishment
				{
					EstablishmentId = 4308714,
					EstablishmentName = "The Noble Hops",
					Lat = -33.8927,
					Long = 151.202,
					BaseZoom = 18,
					LocationId = 3
				},
				new Establishment
				{
					EstablishmentId = 6066172,
					EstablishmentName = "Malt Traders - Southbank",
					Lat = -27.4799,
					Long = 153.023,
					BaseZoom = 17,
					LocationId = 1
				},
				new Establishment
				{
					EstablishmentId = 7767345,
					EstablishmentName = "Tippler's Tap",
					Lat = -27.4579,
					Long = 153.042,
					BaseZoom = 17,
					LocationId = 1
				},
				new Establishment
				{
					EstablishmentId = 7918310,
					EstablishmentName = "Hellbound",
					Lat = -27.4645,
					Long = 153.042,
					BaseZoom = 17,
					LocationId = 1
				},
				new Establishment
				{
					EstablishmentId = 9136453,
					EstablishmentName = "The Woods Bar",
					Lat = -27.412,
					Long = 152.975,
					BaseZoom = 17,
					LocationId = 4
				});

			//modelBuilder.Entity<Style>.HasData(new Style { Id = 1, BaseStyle = "IPA", SimpleStyle = "IPA, Colour = null },
			//	new Style { Id = 2, BaseStyle = "Sour", SimpleStyle = "Sour", Colour = null },
			//	new Style { Id = 3, BaseStyle = "Stout", SimpleStyle = "Stout", Colour = null },
			//	new Style { Id = 4, BaseStyle = "Pale Ale", SimpleStyle = "Pale", Colour = null },
			//	new Style { Id = 5, BaseStyle = "Porter", SimpleStyle = null, Colour = null },
			//	new Style { Id = 6, BaseStyle = "Farmhouse Ale", SimpleStyle = null, Colour = null },
			//	new Style { Id = 7, BaseStyle = "Lager", SimpleStyle = "Lager", Colour = null },
			//	new Style { Id = 8, BaseStyle = "Red Ale", SimpleStyle = null, Colour = null },
			//	new Style { Id = 9, BaseStyle = "Pilsner", SimpleStyle = null, Colour = null },
			//	new Style { Id = 10, BaseStyle = "Brown Ale", SimpleStyle = null, Colour = null },
			//	new Style { Id = 11, BaseStyle = "Cider", SimpleStyle = "Cider", Colour = null },
			//	new Style { Id = 12, BaseStyle = "Wheat Beer", SimpleStyle = null, Colour = null },
			//	new Style { Id = 13, BaseStyle = "Lambic", SimpleStyle = null, Colour = null },
			//	new Style { Id = 14, BaseStyle = "Fruit Beer", SimpleStyle = null, Colour = null },
			//	new Style { Id = 15, BaseStyle = "Kölsch", SimpleStyle = null, Colour = null },
			//	new Style { Id = 16, BaseStyle = "Strong Ale", SimpleStyle = null, Colour = null },
			//	new Style { Id = 17, BaseStyle = "Golden Ale", SimpleStyle = null, Colour = null },
			//	new Style { Id = 18, BaseStyle = "Blonde Ale", SimpleStyle = null, Colour = null },
			//	new Style { Id = 19, BaseStyle = "Belgian Tripel", SimpleStyle = "Belgian", Colour = null },
			//	new Style { Id = 20, BaseStyle = "Belgian Dubbel", SimpleStyle = "Belgian", Colour = null },
			//	new Style { Id = 21, BaseStyle = "American Wild Ale", SimpleStyle = null, Colour = null },
			//	new Style { Id = 22, BaseStyle = "Barleywine", SimpleStyle = "Barleywine", Colour = null },
			//	new Style { Id = 23, BaseStyle = "Belgian Strong Golden Ale", SimpleStyle = "Belgian", Colour = null },
			//	new Style { Id = 24, BaseStyle = "Table Beer", SimpleStyle = null, Colour = null },
			//	new Style { Id = 25, BaseStyle = "Rauchbier", SimpleStyle = null, Colour = null },
			//	new Style { Id = 26, BaseStyle = "Extra Special / Strong Bitter", SimpleStyle = null, Colour = null },
			//	new Style { Id = 27, BaseStyle = "Ginger Beer", SimpleStyle = "Other", Colour = null },
			//	new Style { Id = 28, BaseStyle = "Non-Alcoholic Beer", SimpleStyle = "Other", Colour = null },
			//	new Style { Id = 29, BaseStyle = "Kellerbier / Zwickelbier", SimpleStyle = null, Colour = null },
			//	new Style { Id = 30, BaseStyle = "Belgian Quadrupel", SimpleStyle = "Belgian", Colour = null },
			//	new Style { Id = 31, BaseStyle = "Belgian Strong Dark Ale", SimpleStyle = "Belgian", Colour = null },
			//	new Style { Id = 32, BaseStyle = "Bière de Champagne / Bière Brut", SimpleStyle = null, Colour = null },
			//	new Style { Id = 33, BaseStyle = "English Bitter", SimpleStyle = null, Colour = null },
			//	new Style { Id = 34, BaseStyle = "English Mild Ale", SimpleStyle = null, Colour = null },
			//	new Style { Id = 35, BaseStyle = "Cream Ale", SimpleStyle = null, Colour = null },
			//	new Style { Id = 36, BaseStyle = "Hard Seltzer", SimpleStyle = "Other", Colour = null },
			//	new Style { Id = 37, BaseStyle = "Hefeweizen", SimpleStyle = null, Colour = null },
			//	new Style { Id = 38, BaseStyle = "Old Ale", SimpleStyle = null, Colour = null },
			//	new Style { Id = 39, BaseStyle = "Other", SimpleStyle = "Other", Colour = null },
			//	new Style { Id = 40, BaseStyle = "Pumpkin / Yam Beer", SimpleStyle = "Other", Colour = null },
			//	new Style { Id = 41, BaseStyle = "Wild Ale", SimpleStyle = null, Colour = null },
			//	new Style { Id = 42, BaseStyle = "Scotch Ale / Wee Heavy", SimpleStyle = null, Colour = null },
			//	new Style { Id = 43, BaseStyle = "Smoked Beer", SimpleStyle = null, Colour = null },
			//	new Style { Id = 44, BaseStyle = "Shandy / Radler", SimpleStyle = "Other", Colour = null },
			//	new Style { Id = 45, BaseStyle = "Specialty Grain", SimpleStyle = null, Colour = null },
			//	new Style { Id = 46, BaseStyle = "Winter Warmer", SimpleStyle = null, Colour = null },
			//	new Style { Id = 47, BaseStyle = "Rye Beer", SimpleStyle = null, Colour = null },
			//	new Style { Id = 48, BaseStyle = "Mead", SimpleStyle = "Other", Colour = null },
			//	new Style { Id = 49, BaseStyle = "Grisette", SimpleStyle = null, Colour = null },
			//	new Style { Id = 50, BaseStyle = "Gruit / Ancient Herbed Ale", SimpleStyle = null, Colour = null },
			//	new Style { Id = 51, BaseStyle = "Freeze-Distilled Beer", SimpleStyle = null, Colour = null },
			//	new Style { Id = 52, BaseStyle = "Dark Ale", SimpleStyle = null, Colour = null },
			//	new Style { Id = 53, BaseStyle = "Bock", SimpleStyle = null, Colour = null },
			//	new Style { Id = 54, BaseStyle = "Belgian Blonde", SimpleStyle = "Belgian", Colour = null },
			//	new Style { Id = 55, BaseStyle = "Australian Sparkling Ale", SimpleStyle = null, Colour = null });
		}
	}
}
