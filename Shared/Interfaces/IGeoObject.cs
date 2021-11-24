using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigBeerData.Shared.Interfaces
{
	public interface IGeoObject
	{
		double Long { get; set; }
		double Lat { get; set; }
		//public virtual GeoPoint GeoLocation
		//{
		//	get
		//	{
		//		return new GeoPoint { Y = this.Lat, X = this.Long };
		//	}
		//}
	}
}
