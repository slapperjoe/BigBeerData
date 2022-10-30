namespace BigBeerData.Shared.DTOs
{
	public class BeerDTO
	{
		public int tapNo { get; set; } = default!;
		public string name { get; set; } = default!;
		public string brewer { get; set; } = default!;
		public string description { get; set; } = default!;
		public decimal schooner { get; set; } = default!;
		public decimal squealer { get; set; } = default!;
		public decimal growler { get; set; } = default!;

		public string beerType { get; set; } = default!;
		public decimal percentage { get; set; } = default!;

		public bool changed
		{
			get;
			set;
		}
	}
}
