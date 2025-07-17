using Api.Dtos;

namespace Api.Interfaces
{
    public interface IArticleServices
    {
        Task<IEnumerable<GetArticleDto>> GetPublicArticlesByUserIdAsync(string userId);

        Task<IEnumerable<GetByUserArticleDto>> GetMyArticlesAsync(string userId);

        Task<IEnumerable<GetArticleDto>> GetAllPublicArticlesAsync(string userId);
        Task<GetArticleDto> GetArticleByTitleAndUsernameAsync(string username, string title);

        Task<GetByUserArticleDto> CreateArticleAsync(CreateArticleDto createArticleDto, string userId);

        Task<GetByUserArticleDto> UpdateArticleAsync(UpdateArticleDto updateArticleDto, string userId);

        Task<bool> DeleteArticleAsync(string userId, Guid articleId);
        Task<IEnumerable<GetArticleDto>> GetArticlesBySearchAsync(string searchTerm, string userId);
        Task<bool> LikeArticleAsync(Guid articleId, string userId);
        Task<bool> UnlikeArticleAsync(Guid articleId, string userId);



    }
}