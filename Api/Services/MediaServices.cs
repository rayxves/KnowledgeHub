using Api.Data;
using Api.Dtos;
using Api.Interfaces;
using Api.Mappers;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using Supabase.Storage;
using FileOptions = Supabase.Storage.FileOptions;

namespace Api.Services
{
    public class MediaServices : IMediaServices
    {
        private readonly ApplicationDbContext _context;
        private readonly Supabase.Client _supabaseClient;
        private readonly IConfiguration _configuration;

        public MediaServices(ApplicationDbContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _supabaseClient = new Supabase.Client(
                _configuration["Supabase:Url"] ?? throw new InvalidOperationException("Supabase URL não configurado."),
                _configuration["Supabase:ServiceRole"] ?? throw new InvalidOperationException("Supabase ServiceRole não configurado.")
            );
            _context = context;

        }

        private static async Task<byte[]> ReadStreamToByteArrayAsync(Stream inputStream)
        {
            using var memoryStream = new MemoryStream();
            await inputStream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        public async Task<bool> DeleteMediaFromArticleAsync(Guid articleId, Guid mediaId, string userId)
        {
            var media = await _context.MediaItems
                .Include(m => m.Article)
                .FirstOrDefaultAsync(m => m.Id == mediaId && m.ArticleId == articleId);

            if (media == null || media.Article.UserId != userId)
            {
                throw new InvalidOperationException("Mídia não encontrada ou usuário não autorizado a deletar esta mídia.");
            }

            var deleteResult = await _supabaseClient.Storage
                .From("imagens-artigos")
                .Remove(media.Url);

            if (deleteResult == null)
            {
                throw new InvalidOperationException("Erro ao remover o arquivo do Supabase.");
            }

            _context.MediaItems.Remove(media);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<MediaDto>> GetMediaByArticleIdAsync(Guid articleId)
        {
           var mediaItems = await _context.MediaItems
                .Where(m => m.ArticleId == articleId)
                .ToListAsync();

            if (mediaItems == null || !mediaItems.Any())
            {
                return Enumerable.Empty<MediaDto>();
            }

            return mediaItems.Select(m => m.ToMediaDto()).ToList();
        }
        public async Task<MediaDto> CreateMediaFromFileAsync(IFormFile file, string description)
        {

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var fileBytes = await ReadStreamToByteArrayAsync(file.OpenReadStream());
            var imagePath = await _supabaseClient.Storage
                .From("imagens-artigos")
                .Upload(fileBytes, fileName, new FileOptions
                {
                    ContentType = file.ContentType,
                    CacheControl = "3600"
                });

            var mediaType = MapMimeTypeToMediaType(file.ContentType);

            var media = new MediaDto
            {
                Url = imagePath,
                Type = mediaType.ToString(),
                Description = description,

            };

            return media;
        }

        public static MediaType MapMimeTypeToMediaType(string mimeType)
        {
            if (string.IsNullOrEmpty(mimeType))
                return MediaType.Other;

            if (mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return MediaType.Image;

            if (mimeType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
                return MediaType.Video;

            if (mimeType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
                return MediaType.Audio;

            return MediaType.Other;
        }

        public async Task<MediaDto> GetMediaByUrlAndUserIdAsync(string url, string userId)
        {
            var media = await _context.MediaItems
                .Include(m => m.Article)
                .FirstOrDefaultAsync(m => m.Url == url && m.Article.UserId == userId);

            if (media == null)
            {
                throw new InvalidOperationException("Mídia não encontrada ou usuário não autorizado a acessar esta mídia.");
            }
            
            return media.ToMediaDto();
        }
    }

}