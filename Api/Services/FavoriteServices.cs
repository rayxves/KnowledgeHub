namespace Api.Services
{
    using Api.Data;
    using Api.Dtos;
    using Api.Interfaces;
    using Api.Mappers;
    using Api.Models;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class FavoriteServices : IFavoriteServices
    {
        private readonly ApplicationDbContext _context;
        public FavoriteServices(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<bool> AddToFavoritesAsync(Guid articleId, string userId)
        {
            var article = await _context.Articles.FindAsync(articleId);
            if (article == null)
            {
                throw new KeyNotFoundException("Artigo não foi encontrado.");
            }
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("Usuário não foi encontrado.");
            }

            if (await IsArticleFavoritedAsync(articleId, userId))
            {
                throw new InvalidOperationException("Artigo já está nos favoritos.");
            }
            var favorite = new Favorite
            {
                ArticleId = articleId,
                Article = article,
                User = user,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            user.Favorites.Add(favorite);
            _context.Favorites.Add(favorite);
            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> RemoveFromFavoritesAsync(Guid articleId, string userId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.ArticleId == articleId && f.UserId == userId);
            if (favorite == null)
            {
                throw new KeyNotFoundException("Favorito não encontrado.");
            }

            _context.Favorites.Remove(favorite);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<IEnumerable<GetByUserArticleDto>> GetFavoriteArticlesAsync(string userId)
        {
            var favorites = await _context.Favorites
                .Include(f => f.Article)
                .Include(f => f.User)
                .Where(f => f.UserId == userId)
                .Select(f => f.ArticleId)
                .ToListAsync();
                
            if (favorites == null || !favorites.Any())
            {
                throw new KeyNotFoundException("Nenhum favorito encontrado para este usuário.");
            }
            var articles = await _context.Articles
                .Where(a => favorites.Contains(a.Id))
                .Include(a => a.User)
                .Include(a => a.Category)
                .Include(a => a.Likes)
                .Include(a => a.Comments)
                .Include(a => a.MediaItems)
                .ToListAsync();

            return articles.Select(a => new GetByUserArticleDto
            {
                Id = a.Id,
                Title = a.Title,
                ContentHtmlSanitized = a.ContentHtmlSanitized,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                CreatedBy = a.User.UserName,
                CategoryName = a.Category?.Name ?? "Uncategorized",
                Status = a.Status.ToString(),
                LikesCount = a.Likes.Count,
                UserLiked = a.Likes.Any(l => l.UserId == userId),
                Comments = a.Comments?
                    .Where(c => c.ParentCommentId == null)
                    .Select(c => c.ToGetCommentDto())
                    .ToList() ?? new List<GetCommentDto>(),
                MediaItems = a.MediaItems?.Select(m => m.ToMediaDto()).ToList() ?? new List<MediaDto>()
                });
        }
        public async Task<bool> IsArticleFavoritedAsync(Guid articleId, string userId)
        {
            return await _context.Favorites
                .AnyAsync(f => f.ArticleId == articleId && f.UserId == userId);
        }
    }
}