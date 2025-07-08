using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public enum MediaType { Image, Video, Audio, Other }

public class Media
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Url { get; set; } = string.Empty;

    public MediaType Type { get; set; }

    public string? Description { get; set; }

    public Guid ArticleId { get; set; }
    public Article Article { get; set; } = null!;
}

}