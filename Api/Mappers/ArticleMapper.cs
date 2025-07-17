// File: Mappers/ArticleMapper.cs
using Api.Data;
using Api.Dtos;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Mappers
{
    public static class ArticleMapper
    {
        public static GetArticleDto ToGetArticleDtoAsync(this Article article, string userId)
        {
            return new GetArticleDto
            {
                Id = article.Id,
                Title = article.Title,
                ContentHtmlSanitized = article.ContentHtmlSanitized,
                CreatedAt = article.CreatedAt,
                CreatedBy = article.User?.UserName ?? "Unknown",
                CategoryName = article.Category?.Name ?? "Uncategorized",
                LikesCount = article.Likes.Count,
                UserLiked = article.Likes.Any(l => l.UserId == userId),
                Comments = article.Comments?
                    .Where(c => c.ParentCommentId == null)
                    .Select(c => c.ToGetCommentDto())
                    .ToList() ?? new List<GetCommentDto>(),
                MediaItems = article.MediaItems?.Select(m => m.ToMediaDto()).ToList() ?? new List<MediaDto>()
            };
        }

        public static GetByUserArticleDto ToGetByUserArticleDtoAsync(this Article article, string userId)
        {
            return new GetByUserArticleDto
            {
                Id = article.Id,
                Title = article.Title,
                ContentHtmlSanitized = article.ContentHtmlSanitized,
                CreatedAt = article.CreatedAt,
                CreatedBy = article.User?.UserName ?? "Unknown",
                CategoryName = article.Category?.Name ?? "Uncategorized",
                Status = article.Status.ToString(),
                UpdatedAt = article.UpdatedAt,
                LikesCount = article.Likes.Count,
                UserLiked = article.Likes.Any(l => l.UserId == userId),
                Comments = article.Comments?
                    .Where(c => c.ParentCommentId == null)
                    .Select(c => c.ToGetCommentDto())
                    .ToList() ?? new List<GetCommentDto>(),
                MediaItems = article.MediaItems?.Select(m => m.ToMediaDto()).ToList() ?? new List<MediaDto>()
            };
        }
    }
}