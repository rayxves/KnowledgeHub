using Api.Dtos;

namespace Api.Interfaces
{
    public interface IMediaServices
    {
        Task<bool> DeleteMediaFromArticleAsync(Guid articleId, Guid mediaId, string userId);

        Task<IEnumerable<MediaDto>> GetMediaByArticleIdAsync(Guid articleId);

        Task<MediaDto> CreateMediaFromFileAsync(IFormFile file, string description);
        
        Task<MediaDto> GetMediaByUrlAndUserIdAsync(string url, string userId);
    }
}