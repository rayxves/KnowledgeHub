using Api.Models;

namespace Api.Dtos
{
    public class MediaDto
    {
        public string Url { get; set; } = string.Empty;

        public string Type { get; set; }

        public string? Description { get; set; }
    }
}