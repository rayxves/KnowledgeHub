using Api.Models;

namespace Api.Dtos
{
    public class GetArticleVersionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ContentHtmlSanitized { get; set; }
        public DateTime EditedAt { get; set; }
        public string? EditedByUser { get; set; }
        public int VersionNumber { get; set; }
        public ICollection<MediaDto>? MediaItems { get; set; }
    }

}