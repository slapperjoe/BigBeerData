namespace BigBeerData.Shared.DTOs
{
	public class BeerDTO
	{
		public int tapNo { get; set; } = default!;
		public string name { get; set; } = default!;
		public string brewer { get; set; } = default!;
		public string description { get; set; } = default!;
		public double schooner { get; set; } = default!;
		public double squealer { get; set; } = default!;
		public double growler { get; set; } = default!;

		public string beerType { get; set; } = default!;
		public double percentage { get; set; } = 5;

		public bool changed
		{
			get;
			set;
		}
	}
}
