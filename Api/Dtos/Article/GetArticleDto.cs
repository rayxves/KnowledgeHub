
using Api.Models;

namespace Api.Dtos
{
    public class GetArticleDto
    {
        public string Title { get; set; } = string.Empty;

        public string? ContentHtmlSanitized { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string CreatedBy { get; set; } = default!;

        public string CategoryName { get; set; } = default!;

        public int LikesCount { get; set; }

        public bool UserLiked { get; set; }

        public List<GetCommentDto>? Comments { get; set; } 

        public ICollection<Media>? MediaItems { get; set; }
    }
}