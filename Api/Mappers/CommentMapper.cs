using Api.Dtos;
using Api.Models;

namespace Api.Mappers
{
    public static class CommentMapper
    {
        public static GetCommentDto ToGetCommentDto(this Comment comment)
        {
            return new GetCommentDto
            {
                Text = comment.Text,
                CreatedAt = comment.CreatedAt,
                CreatedBy = comment.User?.UserName ?? "Unknown",
                ParentCommentId = comment.ParentCommentId,
                LikesCount = comment.Likes.Count,
                UserLiked = comment.Likes.Any(l => l.UserId == comment.UserId),
                Replies = comment.Replies.Select(r => r.ToGetCommentDto()).ToList()
            };
        }
    }
}