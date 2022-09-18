namespace Client.DTOs
{
    public class ContentDto
    {
        public Stream Content { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string ContentType { get; set; } = default!;
    }
}
