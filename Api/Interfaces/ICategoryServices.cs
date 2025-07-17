using Api.Dtos;

namespace Api.Interfaces
{
    public interface ICategoryServices
    {
        Task<IEnumerable<GetArticleDto>> GetArticlesByCategoryAsync(string slug);
        Task<IEnumerable<GetArticleDto>> GetArticleByCategoryAndUserIdAsync(string slug, string userId);
        
        Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
        
        
    }
}