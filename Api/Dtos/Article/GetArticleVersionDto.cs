namespace Api.Dtos
{
    public class GetArticleVersionDto
    {
        public string Title { get; set; } = string.Empty;
        public string? ContentHtmlSanitized { get; set; }
        public DateTime EditedAt { get; set; }
        public string? EditedByUser { get; set; }
    }

}