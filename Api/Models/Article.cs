using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models
{
    public enum ArticleStatus
    {
        Draft,
        Published,
        Archived
    }

    public class Article
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string ContentMarkdown { get; set; } = string.Empty;

        public string? ContentHtmlSanitized { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ArticleStatus Status { get; set; } = ArticleStatus.Draft;

        public string UserId { get; set; } = string.Empty;

        public User User { get; set; } = default!;

        public Guid CategoryId { get; set; }

        public Category Category { get; set; } = default!;

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<ArticleLike> Likes { get; set; } = new List<ArticleLike>();

        [NotMapped]
        public List<string> MediaUrls { get; set; } = new List<string>();
    }
}