namespace Api.Dtos
{
    public class GetCommentDto
    {
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string Username { get; set; } = string.Empty;

        public Guid? ParentCommentId { get; set; }

        public int LikesCount { get; set; }

        public bool UserLiked { get; set; }

        public List<GetCommentDto> Replies { get; set; } = new();
    }
}
