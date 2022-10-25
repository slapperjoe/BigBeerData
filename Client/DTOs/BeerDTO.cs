namespace Client.DTOs
{
	public class BeerDTO
	{
		public string name { get; set; } = default!;
		public string brewer { get; set; } = default!;
		public string description { get; set; } = default!;
		public decimal schooner { get; set; } = default!;
		public decimal squealer { get; set; } = default!;
		public decimal growler { get; set; } = default!;

		public bool changed { get; set; } = true;

		public string visStyle {  
			get
			{
				if (changed)
				{
					return "display: none";

				} 
				return "";
			} 
		}
	}
}
