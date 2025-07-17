using Api.Data;
using Api.Dtos;
using Api.Interfaces;
using Api.Mappers;
using Api.Models;
using Markdig;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Api.Services
{
    public class ArticleServices : IArticleServices
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMongoCollection<ArticleVersion> _articleVersions;

        public ArticleServices(ApplicationDbContext context, UserManager<User> userManager, IMongoClient client, IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            var database = client.GetDatabase(config["MongoDb:DatabaseName"]);
            _articleVersions = database.GetCollection<ArticleVersion>("ArticleVersions");
        }

        public async Task<GetByUserArticleDto> CreateArticleAsync(CreateArticleDto dto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new ArgumentException("Usuário não encontrado.");

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Slug.ToLower() == dto.Slug.ToLower())
                ?? throw new ArgumentException("Categoria não encontrada.");

            var existingArticle = await _context.Articles
                .FirstOrDefaultAsync(a => a.Title.ToLower() == dto.Title.ToLower() && a.UserId == userId)
                ?? throw new ArgumentException("Já existe um artigo desse usuário com esse título.");

            var article = new Article
            {
                Title = dto.Title,
                ContentMarkdown = dto.ContentMarkdown,
                ContentHtmlSanitized = Markdown.ToHtml(dto.ContentMarkdown),
                UserId = userId,
                User = user,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Category = category,
                Status = ArticleStatus.Published,
                Likes = new List<ArticleLike>(),
                Comments = new List<Comment>(),
                Favorites = new List<Favorite>()
            };

            await _context.Articles.AddAsync(article);
            await _context.SaveChangesAsync();

            if (dto.MediaItems?.Any() == true)
            {
                foreach (var mediaItem in dto.MediaItems)
                {
                    _context.MediaItems.Add(new Media
                    {
                        Url = mediaItem.Url,
                        Description = mediaItem.Description,
                        Type = Enum.TryParse<MediaType>(mediaItem.Type, true, out var type) ? type : MediaType.Image,
                        ArticleId = article.Id
                    });
                }

                await _context.SaveChangesAsync();
            }

            return article.ToGetByUserArticleDtoAsync(userId);
        }

        public async Task<bool> DeleteArticleAsync(string userId, Guid articleId)
        {
            var article = await _context.Articles.FirstOrDefaultAsync(a => a.Id == articleId && a.UserId == userId)
                ?? throw new ArgumentException("Artigo não encontrado ou você não tem permissão.");

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<GetArticleDto>> GetAllPublicArticlesAsync(string userId)
        {
            var articles = await _context.Articles
                .Where(a => a.Status == ArticleStatus.Published)
                .Include(a => a.User)
                .Include(a => a.Category)
                .Include(a => a.Comments).ThenInclude(c => c.User)
                .Include(a => a.MediaItems)
                .Include(a => a.Likes)
                .ToListAsync();

            var result = new List<GetArticleDto>();
            foreach (var article in articles)
                result.Add(article.ToGetArticleDtoAsync(userId));

            return result;
        }

        public async Task<GetArticleDto> GetArticleByTitleAndUsernameAsync(string username, string title)
        {
            var user = await _userManager.FindByNameAsync(username)
                ?? throw new ArgumentException("Usuário não encontrado.");
            var article = await _context.Articles
                .Include(a => a.User)
                .Include(a => a.Category)
                .Include(a => a.Comments).ThenInclude(c => c.User)
                .Include(a => a.MediaItems)
                .Include(a => a.Likes)
                .FirstOrDefaultAsync(a => a.Title.ToLower() == title.ToLower() && a.UserId == user.Id && a.Status == ArticleStatus.Published)
                ?? throw new ArgumentException("Artigo não encontrado.");
            return article.ToGetArticleDtoAsync(user.Id);
        }

        public async Task<IEnumerable<GetArticleDto>> GetArticlesBySearchAsync(string searchTerm, string userId)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return [];

            var terms = searchTerm.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var query = _context.Articles
                .Include(a => a.User)
                .Include(a => a.Category)
                .Include(a => a.Comments).ThenInclude(c => c.User)
                .Include(a => a.MediaItems)
                .Include(a => a.Likes)
                .Where(a => a.Status == ArticleStatus.Published);

            foreach (var term in terms)
            {
                var t = term;
                query = query.Where(a =>
                    EF.Functions.Like(a.Title.ToLower(), $"%{t}%") ||
                    EF.Functions.Like(a.ContentMarkdown.ToLower(), $"%{t}%") ||
                    EF.Functions.Like(a.Category.Name.ToLower(), $"%{t}%"));
            }

            var articles = await query.ToListAsync();

            var result = new List<GetArticleDto>();
            foreach (var article in articles)
                result.Add(article.ToGetArticleDtoAsync(userId));

            return result;
        }

        public async Task<IEnumerable<GetByUserArticleDto>> GetMyArticlesAsync(string userId)
        {
            var articles = await _context.Articles
                .Where(a => a.UserId == userId)
                .Include(a => a.User)
                .Include(a => a.Category)
                .Include(a => a.Comments).ThenInclude(c => c.User)
                .Include(a => a.MediaItems)
                .Include(a => a.Likes)
                .ToListAsync();

            var result = new List<GetByUserArticleDto>();
            foreach (var article in articles)
                result.Add(article.ToGetByUserArticleDtoAsync(userId));

            return result;
        }

        public async Task<IEnumerable<GetArticleDto>> GetPublicArticlesByUserIdAsync(string userId)
        {
            var articles = await _context.Articles
                .Where(a => a.UserId == userId && a.Status == ArticleStatus.Published)
                .Include(a => a.User)
                .Include(a => a.Category)
                .Include(a => a.Comments).ThenInclude(c => c.User)
                .Include(a => a.MediaItems)
                .Include(a => a.Likes)
                .ToListAsync();

            var result = new List<GetArticleDto>();
            foreach (var article in articles)
                result.Add(article.ToGetArticleDtoAsync(userId));

            return result;
        }



        public async Task<bool> LikeArticleAsync(Guid articleId, string userId)
        {
            var article = await _context.Articles
                .Include(a => a.Likes)
                .FirstOrDefaultAsync(a => a.Id == articleId)
                ?? throw new ArgumentException("Artigo não encontrado.");

            if (article.Likes.Any(l => l.UserId == userId)) return false;

            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new ArgumentException("Usuário não encontrado.");

            var like = new ArticleLike
            {
                ArticleId = article.Id,
                UserId = userId,
                LikedAt = DateTime.UtcNow,
                User = user,
                Article = article
            };

            _context.ArticleLikes.Add(like);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UnlikeArticleAsync(Guid articleId, string userId)
        {
            var article = await _context.Articles
                .Include(a => a.Likes)
                .FirstOrDefaultAsync(a => a.Id == articleId)
                ?? throw new ArgumentException("Artigo não encontrado.");

            var like = article.Likes.FirstOrDefault(l => l.UserId == userId)
                ?? throw new ArgumentException("Curtida não encontrada.");

            _context.ArticleLikes.Remove(like);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<GetByUserArticleDto> UpdateArticleAsync(UpdateArticleDto dto, string userId)
        {
            var article = await _context.Articles
                .Include(a => a.User)
                .Include(a => a.Category)
                .Include(a => a.MediaItems)
                .Include(a => a.Likes)
                .Include(a => a.Comments)
                .FirstOrDefaultAsync(a => a.Id == dto.Id);

            if (article == null || article.UserId != userId)
                throw new ArgumentException("Artigo não encontrado ou sem permissão.");

            var status = Enum.TryParse<ArticleStatus>(dto.Status, true, out var parsedStatus)
                ? parsedStatus : throw new ArgumentException("Status inválido.");

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Slug.ToLower() == dto.Slug.ToLower())
                ?? throw new ArgumentException("Categoria não encontrada.");


            var version = new ArticleVersion
            {
                Title = dto.Title,
                ArticleId = article.Id,
                EditedAt = DateTime.UtcNow,
                EditedByUserId = userId,
                ContentMarkdown = article.ContentMarkdown,
                ContentHtmlSanitized = article.ContentHtmlSanitized,
                VersionNumber = (int)await _articleVersions.CountDocumentsAsync(av => av.ArticleId == article.Id) + 1,
            };

            var mediaVersions = new List<MediaVersion>();
            foreach (var MediaItem in dto.MediaItems ?? [])
            {
                var mediVersion = new MediaVersion
                {
                    Url = MediaItem.Url,
                    Type = Enum.TryParse<MediaType>(MediaItem.Type, true, out var type) ? type : MediaType.Image,
                    Description = MediaItem.Description,
                    ArticleVersionId = version.Id,
                    ArticleVersion = version
                };
                mediaVersions.Add(mediVersion);
            }

            version.MediaItems = mediaVersions;

            await _articleVersions.InsertOneAsync(version);

            article.Title = dto.Title;
            article.ContentMarkdown = dto.ContentMarkdown;
            article.ContentHtmlSanitized = Markdown.ToHtml(dto.ContentMarkdown);
            article.UpdatedAt = DateTime.UtcNow;
            article.Status = parsedStatus;
            article.Category = category;

            var oldMedia = await _context.MediaItems.Where(m => m.ArticleId == article.Id).ToListAsync();
            _context.MediaItems.RemoveRange(oldMedia);

            if (dto.MediaItems?.Any() == true)
            {
                foreach (var mediaItem in dto.MediaItems)
                {
                    _context.MediaItems.Add(new Media
                    {
                        Url = mediaItem.Url,
                        Description = mediaItem.Description,
                        Type = Enum.TryParse<MediaType>(mediaItem.Type, true, out var type) ? type : MediaType.Image,
                        ArticleId = article.Id
                    });
                }
            }

            _context.Articles.Update(article);
            await _context.SaveChangesAsync();

            return article.ToGetByUserArticleDtoAsync(userId);
        }
    }
}
