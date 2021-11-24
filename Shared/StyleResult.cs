using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BigBeerData.Shared
{
	public class StyleResult
	{
		
		public string Name { get; set; }
		public int Count { get; set; }
		public string Color { get; set; }

		public int Height { get; set; }
		
	}

	public class LocationStyle
	{
		public string Name { get; set; }
		public GeoPoint Location { get; set; }
		public int Venue { get; set; }
		public List<StyleResult> Styles { get; set; }

		public int Total { get; set; }

	}
}
