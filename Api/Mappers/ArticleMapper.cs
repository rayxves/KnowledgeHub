using Api.Dtos;
using Api.Models;

namespace Api.Mappers
{
    public static class ArticleMapper
    {
        public static GetByUserArticleDto ToArticleDto(this Article article)
        {
            return new GetByUserArticleDto
            {
                Id = article.Id,
                Title = article.Title,
                ContentHtmlSanitized = article.ContentHtmlSanitized,
                CreatedAt = article.CreatedAt,
                CreatedBy = article.User?.UserName ?? "Unknown",
                CategoryName = article.Category?.Name ?? "Uncategorized",
                UpdatedAt = article.UpdatedAt,
                Status = article.Status.ToString(),
                LikesCount = article.Likes.Count,
                Comments = article.Comments?.Select(c => c.ToGetCommentDto()).ToList(),
                MediaItems = article.MediaItems.Select(m => new MediaDto
                {
                  
                    Url = m.Url,
                    Type = m.Type.ToString(),
                    Description = m.Description
                }).ToList() ?? new List<MediaDto>(),
            };
        }
    }
}