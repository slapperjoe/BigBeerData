using BigBeerData.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace BigBeerData.Shared
{
	public class GeoBounds
	{

		public GeoBounds(IEnumerable<IGeoObject> locObjs)
		{
			if (locObjs.Any())
			{
				West = locObjs.Min(a => a.Long);
				East = locObjs.Max(a => a.Long);
				North = locObjs.Max(a => a.Lat);
				South = locObjs.Min(a => a.Lat);
			}
		}

		public double[] ToDoubleArray()
		{
			return new double[] { West, South, East, North };
		}

		public GeoBounds() { }
		public double West { get; set; }
		public double South { get; set; }
		public double East { get; set; }
		public double North { get; set; }

		public double DeltaX
		{
			get
			{
				return this.East - this.West;
			}
		}

		public double CentreX
		{
			get
			{
				return this.West + (this.DeltaX/2);
			}
		}

		public double DeltaY
		{
			get
			{
				return this.North - this.South;
			}
		}

		public double CentreY
		{
			get
			{
				return this.South + (this.DeltaY/2);
			}
		}

		public GeoPoint Middle
		{
			get
			{
				return new GeoPoint { X = this.CentreX, Y = this.CentreY };
			}
		}
	}
}