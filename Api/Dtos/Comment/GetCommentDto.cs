namespace Api.Dtos
{
    public class GetCommentDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        public Guid? ParentCommentId { get; set; }

        public int LikesCount { get; set; }

        public bool UserLiked { get; set; }

        public List<GetCommentDto> Replies { get; set; } = new();
    }
}
