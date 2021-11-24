using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigBeerData.Shared
{
	public static class Extensions
	{
		public static string ToPixel(this int pixels)
		{
			return pixels + "px";
		}
	}
}
