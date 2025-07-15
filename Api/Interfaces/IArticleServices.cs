using Api.Dtos;

namespace Api.Interfaces
{
    public interface IArticleServices
    {
        Task<IEnumerable<GetArticleDto>> GetPublicArticlesByUserIdAsync(string userId);

        Task<IEnumerable<GetByUserArticleDto>> GetMyArticlesAsync(string userId);

        Task<IEnumerable<GetArticleDto>> GetAllPublicArticlesAsync();

        Task<GetByUserArticleDto> CreateArticleAsync(CreateArticleDto createArticleDto, string userId);

        Task<GetByUserArticleDto> UpdateArticleAsync(UpdateArticleDto updateArticleDto, string userId);

        Task<bool> DeleteArticleAsync(string userId, Guid articleId);


    }
}