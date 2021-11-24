using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigBeerData.Shared
{
	public class BrewerResult
	{
		//public int Id { get; set; }
		public string SLUG { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }
		public string URL { get; set; }
		public GeoPoint Location { get; set; }

		public IEnumerable<StyleResult> BeersBrewed {get;set;}
	}
}
