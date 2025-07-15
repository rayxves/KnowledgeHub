using Api.Dtos;

namespace Api.Interfaces
{
    public interface ICategoryServices
    {
        Task<GetArticleDto> GetArticleByCategoryAsync(string slug, string userId);
        
        Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
        
        
    }
}