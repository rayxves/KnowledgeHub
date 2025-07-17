using Api.Dtos;

namespace Api.Interfaces
{
    public interface IFavoriteServices
    {
        Task<bool> AddToFavoritesAsync(Guid articleId, string userId);
        Task<bool> RemoveFromFavoritesAsync(Guid articleId, string userId);
        Task<IEnumerable<GetByUserArticleDto>> GetFavoriteArticlesAsync(string userId);
        Task<bool> IsArticleFavoritedAsync(Guid articleId, string userId);
    }
}
