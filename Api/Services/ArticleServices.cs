using Api.Data;
using Api.Dtos;
using Api.Interfaces;
using Api.Mappers;
using Api.Models;
using Markdig;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class ArticleServices : IArticleServices
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ArticleServices(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<GetByUserArticleDto> CreateArticleAsync(CreateArticleDto createArticleDto)
        {
            var user = await _userManager.FindByIdAsync(createArticleDto.UserId);
            if (user == null) throw new ArgumentException("Usúario não foi encontrado.");

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == createArticleDto.CategoryId);
            if (category == null) throw new ArgumentException("Categoria não foi encontrada.");

            var article = new Article
            {
                Title = createArticleDto.Title,
                ContentMarkdown = createArticleDto.ContentMarkdown,
                ContentHtmlSanitized = Markdown.ToHtml(createArticleDto.ContentMarkdown),
                UserId = createArticleDto.UserId,
                User = user,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Category = category,
                Status = createArticleDto.Status,
                Likes = new List<ArticleLike>(),
                Comments = new List<Comment>(),
                Favorites = new List<Favorite>()
            };

            await _context.Articles.AddAsync(article);
            await _context.SaveChangesAsync();

            if (createArticleDto.MediaItems != null && createArticleDto.MediaItems.Any())
            {
                foreach (var mediaItem in createArticleDto.MediaItems)
                {
                    var media = new Media
                    {
                        Url = mediaItem.Url,
                        Type = mediaItem.Type,
                        ArticleId = article.Id
                    };
                    _context.MediaItems.Add(media);
                }
                await _context.SaveChangesAsync();
            }

            return new GetByUserArticleDto
            {
                Title = article.Title,
                ContentHtmlSanitized = article.ContentHtmlSanitized,
                CreatedAt = article.CreatedAt,
                CreatedBy = user.UserName,
                CategoryName = category.Name,
                Status = article.Status,
                UpdatedAt = article.UpdatedAt,
                LikesCount = article.Likes.Count,
                MediaItems = article.MediaItems?.Select(m => m.ToMediaDto()).ToList() ?? new List<MediaDto>(),
            };
        }

        public async Task<bool> DeleteArticleAsync(string userId, Guid articleId)
        {
            var article = await _context.Articles
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == articleId && a.UserId == userId);

            if (article == null) throw new ArgumentException("Artigo não encontrado ou você não tem permissão para excluí-lo.");

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<GetArticleDto>> GetAllPublicArticlesAsync()
        {
            var articles = await _context.Articles
                .Where(a => a.Status == ArticleStatus.Published)
                .Include(a => a.User)
                .Include(a => a.Category)
                .Include(a => a.Comments).ThenInclude(c => c.User)
                .Include(a => a.MediaItems)
                .ToListAsync();

            return articles.Select(a => new GetArticleDto
            {
                Title = a.Title,
                ContentHtmlSanitized = a.ContentHtmlSanitized,
                CreatedAt = a.CreatedAt,
                CreatedBy = a.User.UserName,
                CategoryName = a.Category.Name,
                LikesCount = a.Likes.Count,
                Comments = a.Comments.Select(c => new GetCommentDto
                {
                    Text = c.Text,
                    CreatedAt = c.CreatedAt,
                    CreatedBy = c.User.UserName,
                    ParentCommentId = c.ParentCommentId
                }).ToList(),
                MediaItems = a.MediaItems.Select(m => new MediaDto
                {
                    Url = m.Url,
                    Type = m.Type,
                    Description = m.Description
                }).ToList()
            });
        }

        public async Task<IEnumerable<GetByUserArticleDto>> GetMyArticlesAsync(string userId)
        {
            var articles = await _context.Articles
                .Where(a => a.UserId == userId)
                .Include(a => a.User)
                .Include(a => a.Category)
                .Include(a => a.Comments).ThenInclude(c => c.User)
                .Include(a => a.MediaItems)
                .ToListAsync();

            return articles.Select(a => new GetByUserArticleDto
            {
                Title = a.Title,
                ContentHtmlSanitized = a.ContentHtmlSanitized,
                CreatedAt = a.CreatedAt,
                CreatedBy = a.User.UserName,
                CategoryName = a.Category.Name,
                UpdatedAt = a.UpdatedAt,
                Status = a.Status,
                LikesCount = a.Likes.Count,
                Comments = a.Comments.Select(c => new GetCommentDto
                {
                    Text = c.Text,
                    CreatedAt = c.CreatedAt,
                    CreatedBy = c.User.UserName,
                    ParentCommentId = c.ParentCommentId
                }).ToList(),
                MediaItems = a.MediaItems.Select(m => new MediaDto
                {
                    Url = m.Url,
                    Type = m.Type,
                    Description = m.Description
                }).ToList()
            });
        }

        public async Task<IEnumerable<GetArticleDto>> GetPublicArticlesByUserIdAsync(string userId)
        {
            var articles = await _context.Articles
                .Where(a => a.UserId == userId && a.Status == ArticleStatus.Published)
                .Include(a => a.User)
                .Include(a => a.Category)
                .Include(a => a.Comments).ThenInclude(c => c.User)
                .Include(a => a.MediaItems)
                .ToListAsync();

            return articles.Select(a => new GetArticleDto
            {
                Title = a.Title,
                ContentHtmlSanitized = a.ContentHtmlSanitized,
                CreatedAt = a.CreatedAt,
                CreatedBy = a.User.UserName,
                CategoryName = a.Category.Name,
                LikesCount = a.Likes.Count,
                Comments = a.Comments.Select(c => new GetCommentDto
                {
                    Text = c.Text,
                    CreatedAt = c.CreatedAt,
                    CreatedBy = c.User.UserName,
                    ParentCommentId = c.ParentCommentId
                }).ToList(),
                MediaItems = a.MediaItems.Select(m => new MediaDto
                {
                    Url = m.Url,
                    Type = m.Type,
                    Description = m.Description
                }).ToList()
            });
        }

        public async Task<GetByUserArticleDto> UpdateArticleAsync(UpdateArticleDto updateArticleDto)
        {
            var article = await _context.Articles
                .Include(a => a.User)
                .Include(a => a.Category)
                .Include(a => a.Likes)
                .Include(a => a.MediaItems)
                .FirstOrDefaultAsync(a => a.Id == updateArticleDto.Id);

            if (article == null || article.UserId != updateArticleDto.UserId)
                throw new ArgumentException("Artigo não encontrado ou você não tem permissão para atualizá-lo.");

            article.Title = updateArticleDto.Title;
            article.ContentMarkdown = updateArticleDto.ContentMarkdown;
            article.ContentHtmlSanitized = Markdown.ToHtml(updateArticleDto.ContentMarkdown);
            article.UpdatedAt = DateTime.UtcNow;
            article.Status = updateArticleDto.Status;

            var oldMedia = await _context.MediaItems.Where(m => m.ArticleId == article.Id).ToListAsync();
            _context.MediaItems.RemoveRange(oldMedia);

            if (updateArticleDto.MediaItems != null && updateArticleDto.MediaItems.Any())
            {
                foreach (var mediaItem in updateArticleDto.MediaItems)
                {
                    var media = new Media
                    {
                        Url = mediaItem.Url,
                        Type = mediaItem.Type,
                        Description = mediaItem.Description,
                        ArticleId = article.Id
                    };
                    _context.MediaItems.Add(media);
                }
            }

            _context.Articles.Update(article);
            await _context.SaveChangesAsync();

            return new GetByUserArticleDto
            {
                Title = article.Title,
                ContentHtmlSanitized = article.ContentHtmlSanitized,
                CreatedAt = article.CreatedAt,
                CreatedBy = article.User.UserName,
                CategoryName = article.Category.Name,
                Status = article.Status,
                UpdatedAt = article.UpdatedAt,
                LikesCount = article.Likes.Count,
                MediaItems = article.MediaItems?.Select(m => m.ToMediaDto()).ToList() ?? new List<MediaDto>(),
            };
        }
    }
}
