namespace Client.DTOs
{
	public class BeerDTO
	{
		public string name { get; set; } = default!;
		public string brewer { get; set; } = default!;
		public string description { get; set; } = default!;
        public decimal schooner { get; set; } = default!;
        public decimal growler { get; set; } = default!;
    }
}
