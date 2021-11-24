using BigBeerData.Shared;
using BigBeerData.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Diagnostics.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore.Design;

namespace BigBeerData.Shared
{

	public class BigBeerDatatFactory : IDesignTimeDbContextFactory<BigBeerContext>
	{
		public BigBeerContext CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<BigBeerContext>();
			optionsBuilder.UseSqlServer(
											 "Data Source=(localdb)\\ProjectsV13;Initial Catalog=BigBeerData.Core;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"
									 );

			return new BigBeerContext(optionsBuilder.Options);
		}
	}
	public partial class BigBeerContext : DbContext
	{
		public BigBeerContext(DbContextOptions<BigBeerContext> context) : base(context)
		{
			this.Database.SetCommandTimeout(300);
		}


		public DbSet<Checkin> Checkins { get; set; }
		public DbSet<Beer> Beers { get; set; }
		public DbSet<Brewer> Brewers { get; set; }
		public DbSet<Establishment> Establishments { get; set; }

		public DbSet<Location> Locations { get; set; }

		public DbSet<BeerFamily> BeerFamily { get; set; }
		public DbSet<BeerType> BeerType { get; set; }
		public DbSet<BeerStyle> BeerStyle { get; set; }

		public DbSet<BeerColour> BeerColour { get; set; }


		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<BeerStyle>()
				.HasOne(e => e.SRMHigh)
				.WithMany(e => e.HighStyles)
				.OnDelete(DeleteBehavior.NoAction);
			modelBuilder.Entity<BeerStyle>()
				.HasOne(e => e.SRMLow)
				.WithMany(e => e.LowStyles)
				.OnDelete(DeleteBehavior.NoAction);
			modelBuilder.Entity<BeerStyle>()
				.HasOne(e => e.Type)
				.WithMany(e => e.Styles)
				.OnDelete(DeleteBehavior.NoAction);
			modelBuilder.Entity<Beer>().Property(p => p.BaseStyle)
				.HasComputedColumnSql("(case when charindex(' - ',[Style])=(0) then [Style] else rtrim(left([Style],charindex(' - ',[Style]))) end)");
			
			LoadData(modelBuilder);

			base.OnModelCreating(modelBuilder);
		}
	}

	public class Checkin
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public double CheckinId { get; set; }

		public DateTime CheckinTime { get; set; }

		[ForeignKey("Establishment")]
		public int EstablishmentId { get; set; }

		[ForeignKey("Beer")]
		public int Bid { get; set; }

		public double? Rating { get; set; }
		public virtual Beer Beer { get; set; }
		public Establishment Establishment { get; set; }
	}

	public class Location
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int LocationId { get; set; }
		public string LocationName { get; set; }


		public virtual ICollection<Establishment> Establishments { get; set; }
	}

	public class Beer
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int Bid { get; set; }

		public string SLUG { get; set; }

		public string BeerName { get; set; }
		public string BeerPic { get; set; }
		public string Style { get; set; }

		public double ABV { get; set; }

		public ICollection<Checkin> Checkins { get; set; }

		[ForeignKey("Brewer")]
		public int BrewerId { get; set; }

		public virtual Brewer Brewer { get; set; }

		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public string BaseStyle { get; private set; }

	}



	public class Brewer : IGeoObject
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int BrewerId { get; set; }

		public string SLUG { get; set; }
		public string BrewerName { get; set; }
		public double Lat { get; set; }
		public double Long { get; set; }
		public string Type { get; set; }
		public string Location { get; set; }
		public string Country { get; set; }
		public string State { get; set; }
		public string City { get; set; }
		public string URL { get; set; }

		public virtual ICollection<Beer> Beers { get; set; }
	}

	public class Establishment : IGeoObject
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int EstablishmentId { get; set; }

		public string EstablishmentName { get; set; }
		public double Lat { get; set; }
		public double Long { get; set; }

		public DateTime? LastCheckinUpdate { get; set; }

		public int BaseZoom { get; set; }

		[ForeignKey("Location")]
		public int LocationId { get; set; }
		public virtual Location Location { get; set; }

		public ICollection<Checkin> Checkins { get; set; }

		public virtual DateTime? OldestCheckinDT
		{
			get
			{
				return this.Checkins.OrderBy(a => a.CheckinTime).FirstOrDefault()?.CheckinTime;
			}
		}

		public virtual DateTime? NewestCheckinDate
		{
			get
			{
				return this.Checkins.OrderByDescending(a => a.CheckinTime).FirstOrDefault()?.CheckinTime.Date;
			}
		}

		public bool MaxedCheckinHistory { get; set; }
	}

	public class BeerFamily
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int ID { get; set; }
		public string Name { get; set; }

		[ForeignKey("BeerType")]
		public int BeerTypeID { get; set; }
		public virtual BeerType Type { get; set; }
	}

	public class BeerType
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int ID { get; set; }
		public string Name { get; set; }

		public virtual ICollection<BeerStyle> Styles { get; set; }
		public virtual ICollection<BeerFamily> Families { get; set; }

	}

	public class BeerStyle
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int ID { get; set; }

		[ForeignKey("BeerFamily")]
		public int FamilyID { get; set; }

		public virtual BeerFamily Family { get; set; }

		public string Name { get; set; }

		[ForeignKey("BeerType")]
		public int TypeID { get; set; }

		public virtual BeerType Type { get; set; }

		public double ABVLow { get; set; }

		public double ABVHigh { get; set; }

		public double IBULow { get; set; }
		public double IBUHigh { get; set; }

		[ForeignKey(nameof(SRMLow))]
		public int SRMLowID { get; set; }
		[ForeignKey(nameof(SRMHigh))]
		public int SRMHighID { get; set; }

		public BeerColour SRMLow { get; set; }
		public BeerColour SRMHigh { get; set; }

		public double OrigGravLow { get; set; }
		public double OrigGravHigh { get; set; }
		public double FinalGravLow { get; set; }
		public double FinalGravHigh { get; set; }

		[ForeignKey("BeerYeast")]
		public int YeastID { get; set; }
		public virtual BeerYeast Yeast { get; set; }
	}

	public class BeerColour
	{

		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int ID { get; set; }
		public string Hex { get; set; }


		[InverseProperty(nameof(BeerStyle.SRMLow))]
		public virtual ICollection<BeerStyle> LowStyles { get; set; }

		[InverseProperty(nameof(BeerStyle.SRMHigh))]
		public virtual ICollection<BeerStyle> HighStyles { get; set; }
	}

	public class BeerYeast
	{

		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int ID { get; set; }
		public string Name { get; set; }

		public string BaseColour { get; set; }

		public virtual ICollection<BeerStyle> Styles { get; set; }
	}
}
