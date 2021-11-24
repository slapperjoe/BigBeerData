using System;
using System.Collections.Generic;
using System.Text;

namespace BigBeerData.Shared
{
	public class UntappdData
	{
		public MetaObject Meta { get; set; }
		public Object[] Notifications { get; set; }
		public UntappdResponse Response {get;set;}
	}

	public class MetaObject
	{
		public int Code { get; set; }
	}

	public class UntappdResponse
	{
		public UntappdCheckins Checkins { get; set; }
	}

	public class UntappdCheckins
	{
		public int Count { get; set; }
		public UntappdCheckinItem[] Items { get; set; }
	}

	public class UntappdCheckinItem
	{
		public int Checkin_id { get; set; }
		public string Created_at { get; set; }
		public double Rating_score { get; set; }
		public string Checkin_comment { get; set; }

		public object User { get; set; }

		public UntappdBeer Beer { get; set; }
		public UntappdBrewery Brewery {get;set;}
		public UntappdVenue Venue { get; set; }

		public object Comments { get; set; }
		public object Toastsvv { get; set; }
		public object Media { get; set; }
		public object Source { get; set; }
		public object Badges { get; set; }
	}

	public class UntappdBeer
	{
		public int Bid { get; set; }
		public string Beer_name { get; set; }
		public double Beer_abv { get; set; }
		public string Beer_label { get; set; }
		public string Beer_slug { get; set; }
		public string Beer_style { get; set; }
		public int Beer_active { get; set; }
		public bool Had_had { get; set; }
	}

	public class UntappdBrewery
	{
		public int Brewery_id { get; set; }
		public string Brewery_name { get; set; }
		public UntappdLocation Location { get; set; }
	}

	public class UntappdVenue
	{
		public int Venue_is { get; set; }
		public string Venue_name { get; set; }
		public UntappdLocation Location { get; set; }
	}

	public class UntappdLocation
	{
		public double Lat { get; set; }
		public double Lng { get; set; }
	}

}
