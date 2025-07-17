using Api.Dtos;

namespace Api.Interfaces
{
    public interface ICommentServices
    {
        Task<IEnumerable<GetCommentDto>> GetCommentsByUserIdAndArticleIdAsync(string userId, Guid articleId);

        Task<GetCommentDto> CreateCommentAsync(CreateCommentDto createCommentDto, string userId);

        Task<GetCommentDto> UpdateCommentAsync(UpdateCommentDto updateCommentDto, string userId);

        Task<bool> DeleteCommentAsync(Guid commentId, string userId);

        Task<bool> LikeCommentAsync(Guid commentId, string userId);

        Task<bool> UnlikeCommentAsync(Guid commentId, string userId);

        Task<IEnumerable<GetCommentDto>> GetCommentsByArticleIdAsync(Guid articleId, string userId);

        Task<IEnumerable<GetCommentDto>> GetCommentsByParentCommentIdAsync(Guid parentCommentId, string userId);
        
        Task<GetCommentDto> GetCommentByUserIdAndTextAsync(string userId, string text);
    }
}