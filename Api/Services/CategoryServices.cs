using Api.Data;
using Api.Dtos;
using Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class CategoryServices : ICategoryServices
    {
        private readonly ApplicationDbContext _context;

        public CategoryServices(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug
                })
                .ToListAsync();
        }

        public async Task<GetArticleDto> GetArticleByCategoryAsync(string slug, string userId)
        {
            var article = await _context.Articles
                .Where(a => a.Category.Slug == slug && a.UserId == userId)
                .Select(a => new GetArticleDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    ContentHtmlSanitized = a.ContentHtmlSanitized,
                    CreatedAt = a.CreatedAt
                })
                .FirstOrDefaultAsync();

            return article != null ? article : throw new InvalidOperationException("Article not found for the given category slug.");
        }
    }
}