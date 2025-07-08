using System.ComponentModel.DataAnnotations;
using Api.Models;

namespace Api.Dtos
{
    public class UpdateArticleDto
    {
        [Required]
        public Guid Id { get; set; }
        public string UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string ContentMarkdown { get; set; } = string.Empty;

        public ArticleStatus Status { get; set; }

        public Guid CategoryId { get; set; }

        public ICollection<Media>? MediaItems { get; set; }
    }
}