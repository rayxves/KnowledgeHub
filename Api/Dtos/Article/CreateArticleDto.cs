using System.ComponentModel.DataAnnotations;
using Api.Models;

namespace Api.Dtos
{
    public class CreateArticleDto
    {

        public Guid Id { get; set; }
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string ContentMarkdown { get; set; } = string.Empty;

        [Required]
        public string Slug { get; set; }

        public ICollection<MediaDto>? MediaItems { get; set; }
    }
}