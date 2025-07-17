using Api.Dtos;

namespace Api.Interfaces
{
    public interface IArticleVersionServices
    {
        Task<IEnumerable<GetArticleVersionDto>> GetArticleVersionsAsync(string articleId, string userId);
        Task<bool> RestoreArticleVersionAsync(Guid articleId, int versionNumber, string userId);
        Task<bool> DeleteArticleVersionAsync(Guid articleId, int versionNumber, string userId);
        


    }
}