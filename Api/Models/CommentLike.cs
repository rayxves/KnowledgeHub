using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models
{
    public class CommentLike
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime LikedAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public Guid CommentId { get; set; } = Guid.Empty;

        [ForeignKey("CommentId")]
        public Comment Comment { get; set; } = null!;
    }
}