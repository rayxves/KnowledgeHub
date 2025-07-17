using Api.Data;
using Api.Dtos;
using Api.Interfaces;
using Api.Mappers;
using Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Api.Services
{
    public class ArticleVersionServices : IArticleVersionServices
    {
        private readonly UserManager<User> _userManager;
        private readonly IMongoCollection<ArticleVersion> _articleVersions;
        private readonly ApplicationDbContext _context;
        public ArticleVersionServices(IMongoClient client, IConfiguration config, UserManager<User> userManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
            var database = client.GetDatabase(config["MongoDb:DatabaseName"]);
            _articleVersions = database.GetCollection<ArticleVersion>("ArticleVersions");
        }
        public async Task<bool> DeleteArticleVersionAsync(Guid articleId, int versionNumber, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new ArgumentException("Usuário não encontrado.");

            var version = await _articleVersions
                .Find(av => av.ArticleId == articleId && av.VersionNumber == versionNumber)
                .FirstOrDefaultAsync();

            if (version == null)
                throw new ArgumentException("Versão não encontrada.");

            if (version.EditedByUserId != userId)
                throw new UnauthorizedAccessException("Usuário não tem permissão para deletar esta versão.");

            await _articleVersions.DeleteOneAsync(av => av.Id == version.Id && av.VersionNumber == versionNumber);
            return true;
        }

        public async Task<IEnumerable<GetArticleVersionDto>> GetArticleVersionsAsync(string articleId, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new ArgumentException("Usuário não encontrado.");

            var versions = await _articleVersions
                .Find(av => av.ArticleId == Guid.Parse(articleId))
                .SortByDescending(av => av.EditedAt)
                .ToListAsync();

            if (versions == null || !versions.Any())
                throw new ArgumentException("Nenhuma versão encontrada.");

            return versions.Select(v => new GetArticleVersionDto
            {
                Id = v.ArticleId,
                Title = v.Title,
                ContentHtmlSanitized = v.ContentHtmlSanitized,
                EditedAt = v.EditedAt,
                EditedByUser = user.UserName,
                VersionNumber = v.VersionNumber,
                MediaItems = v.MediaItems?.Select(m => m.ToMediaVersionDto()).ToList() ?? []
            });
        }

        public async Task<bool> RestoreArticleVersionAsync(Guid articleId, int versionNumber, string userId)
        {
            var article = await _context.Articles
                .Include(a => a.User)
                .Include(a => a.Category)
                .Include(a => a.MediaItems)
                .Include(a => a.Likes)
                .Include(a => a.Comments)
                .FirstOrDefaultAsync(a => a.Id == articleId && a.UserId == userId);

            if (article == null)
                throw new ArgumentException("Artigo não encontrado ou usuário não tem permissão para acessar esse artigo.");

            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new ArgumentException("Usuário não encontrado.");

            var version = await _articleVersions
                .Find(av => av.ArticleId == articleId && av.VersionNumber == versionNumber)
                .FirstOrDefaultAsync();

            if (version == null)
                throw new ArgumentException("Versão não encontrada.");

            article.Title = version.Title;
            article.ContentHtmlSanitized = version.ContentHtmlSanitized;
            article.ContentMarkdown = version.ContentMarkdown;
            article.UpdatedAt = DateTime.UtcNow;

            if (article.MediaItems != null)
            {
                _context.MediaItems.RemoveRange(article.MediaItems);
            }

            if (version.MediaItems != null)
            {
                var mediaItems = new List<Media>();
                foreach (var mediaItem in version.MediaItems)
                {
                    mediaItems.Add(new Media
                    {
                        Url = mediaItem.Url,
                        Type = mediaItem.Type,
                        Description = mediaItem.Description,
                        ArticleId = article.Id,
                        Article = article,
                    });
                }
                article.MediaItems = mediaItems;
                _context.MediaItems.AddRange(mediaItems);
            }

            _context.Articles.Update(article);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}