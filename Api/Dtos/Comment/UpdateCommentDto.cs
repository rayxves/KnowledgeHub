using System.ComponentModel.DataAnnotations;

namespace Api.Dtos
{
    public class UpdateCommentDto
{
    [Required]
    public Guid Id { get; set; }

    [Required, StringLength(1000)]
    public string Text { get; set; } = string.Empty;
}
}