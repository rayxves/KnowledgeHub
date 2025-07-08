using System.ComponentModel.DataAnnotations;

namespace Api.Dtos
{
    public class CreateCommentDto
{
    [Required, StringLength(1000)]
    public string Text { get; set; } = string.Empty;

    [Required]
    public Guid ArticleId { get; set; }

    public Guid? ParentCommentId { get; set; }
}

}